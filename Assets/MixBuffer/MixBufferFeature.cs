using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

// Context item class stored in the frameData
public sealed class MixBufferContextItem : ContextItem, System.IDisposable
{
    // Canvas RT: Persistent RTHandle used for frame accumulation
    RTHandle _canvasRT;

    // Custom data for the composite pass
    class PassData
    {
        public TextureHandle source;
        public TextureHandle canvas;
        public Material material;
    }

    public override void Reset() {}

    public void RecordPasses
      (RenderGraph renderGraph,
       ContextContainer frameData,
       Material material)
    {
        // MixBufferController reference
        var camera = frameData.Get<UniversalCameraData>();
        var ctrl = camera.camera.GetComponent<MixBufferController>();
        if (ctrl == null || !ctrl.enabled || !ctrl.IsReady) return;

        // Canvas RT (re)allocation
        var rtDesc = camera.cameraTargetDescriptor;

        rtDesc.msaaSamples = 1;
        rtDesc.depthStencilFormat = GraphicsFormat.None;

        RenderingUtils.ReAllocateHandleIfNeeded
          (ref _canvasRT, rtDesc,
           wrapMode: TextureWrapMode.Clamp, name: "MixBuffer Canvas");

        var canvas = renderGraph.ImportTexture(_canvasRT);

        // Source (camera target texture)
        var resource = frameData.Get<UniversalResourceData>();
        var source = resource.activeColorTexture;

        // Destination texture allocation
        var desc = renderGraph.GetTextureDesc(source);
        desc.name = "MixBuffer Dest";
        desc.clearBuffer = false;
        var dest = renderGraph.CreateTexture(desc);

        // Composite pass setup: source + canvas -> dest
        using (var builder = renderGraph.AddRasterRenderPass<PassData>
          ("MixBuffer Composite", out var passData))
        {
            passData.source = source;
            passData.canvas = canvas;
            passData.material = material;

            builder.UseTexture(passData.source);
            builder.UseTexture(passData.canvas);
            builder.SetRenderAttachment(dest, 0);

            builder.SetRenderFunc
              ((PassData data, RasterGraphContext ctx) => ExecutePass(data, ctx));
        }

        // Copy pass: dest -> canvas
        renderGraph.AddCopyPass(dest, canvas, passName: "MixBuffer Copy Canvas");

        // Use the destination texture as the new camera color.
        resource.cameraColor = dest;
    }

    static void ExecutePass(PassData data, RasterGraphContext context)
    {
        data.material.SetTexture("_BufferTex", data.canvas);
        data.material.SetTexture("_BlitTexture", data.source);
        CoreUtils.DrawFullScreen(context.cmd, data.material);
    }

    public void Dispose()
      => _canvasRT?.Release();
}

sealed class MixBufferPass : ScriptableRenderPass
{
    Material _material;

    public MixBufferPass(Material material)
      => _material = material;

    public override void RecordRenderGraph
      (RenderGraph renderGraph, ContextContainer frameData)
    {
        var contextItem = frameData.GetOrCreate<MixBufferContextItem>();
        contextItem.RecordPasses(renderGraph, frameData, _material);
    }
}

public sealed class MixBufferFeature : ScriptableRendererFeature
{
    [SerializeField, HideInInspector] Shader _shader = null;

    Material _material;
    MixBufferPass _pass;

    public override void Create()
    {
        _material = CoreUtils.CreateEngineMaterial(_shader);
        _pass = new MixBufferPass(_material);
        _pass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    protected override void Dispose(bool disposing)
      => CoreUtils.Destroy(_material);

    public override void AddRenderPasses
      (ScriptableRenderer renderer, ref RenderingData data)
    {
        if (data.cameraData.cameraType != CameraType.Game) return;
        renderer.EnqueuePass(_pass);
    }
}

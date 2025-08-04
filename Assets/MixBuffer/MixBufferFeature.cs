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
    (RTHandle rt1, RTHandle rt2, bool swap) _canvas;

    // Custom data for the composite pass
    class PassData
    {
        public TextureHandle source;
        public TextureHandle canvas;
        public Material material;
    }

    // Empty implementation of abstract Reset()
    public override void Reset() {}

    // Pass building
    public void RecordPasses
      (RenderGraph renderGraph,
       ContextContainer frameData,
       Material material)
    {
        var camera = frameData.Get<UniversalCameraData>();
        var resource = frameData.Get<UniversalResourceData>();

        // MixBufferController reference
        var ctrl = camera.camera.GetComponent<MixBufferController>();
        if (ctrl == null || !ctrl.enabled || !ctrl.IsReady) return;

        // Canvas RT (re)allocation
        var rtDesc = camera.cameraTargetDescriptor;

        rtDesc.msaaSamples = 1;
        rtDesc.depthStencilFormat = GraphicsFormat.None;

        RenderingUtils.ReAllocateHandleIfNeeded
          (ref _canvas.rt1, rtDesc,
           wrapMode: TextureWrapMode.Clamp, name: "MixBuffer Canvas 1");

        RenderingUtils.ReAllocateHandleIfNeeded
          (ref _canvas.rt2, rtDesc,
           wrapMode: TextureWrapMode.Clamp, name: "MixBuffer Canvas 2");

        // RT selection and swapping logic
        var canvas = renderGraph.ImportTexture(_canvas.rt1);
        var dest = renderGraph.ImportTexture(_canvas.rt2);
        if (_canvas.swap) (canvas, dest) = (dest, canvas);
        _canvas.swap = !_canvas.swap;

        // Composite pass setup: source + canvas -> dest
        using (var builder = renderGraph.AddRasterRenderPass<PassData>
          ("MixBuffer Composite", out var passData))
        {
            passData.source = resource.activeColorTexture;
            passData.canvas = canvas;
            passData.material = material;

            builder.UseTexture(passData.source);
            builder.UseTexture(passData.canvas);
            builder.SetRenderAttachment(dest, 0);

            builder.SetRenderFunc
              ((PassData data, RasterGraphContext ctx) => ExecutePass(data, ctx));
        }

        // Use the destination texture as the new camera color.
        resource.cameraColor = dest;
    }

    // Render pass execution
    static void ExecutePass(PassData data, RasterGraphContext context)
    {
        data.material.SetTexture("_MainTex", data.source);
        data.material.SetTexture("_BufferTex", data.canvas);
        CoreUtils.DrawFullScreen(context.cmd, data.material);
    }

    // IDisposable implementation
    public void Dispose()
    {
        _canvas.rt1?.Release();
        _canvas.rt2?.Release();
    }
}

// Render pass class: Simple wrapper for the context item class
sealed class MixBufferPass : ScriptableRenderPass
{
    Material _material;

    public MixBufferPass(Material material)
      => _material = material;

    public override void RecordRenderGraph
      (RenderGraph renderGraph, ContextContainer frameData)
      => frameData.GetOrCreate<MixBufferContextItem>().
           RecordPasses(renderGraph, frameData, _material);
}

// Renderer feature class
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

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

sealed class MixBufferPass : ScriptableRenderPass
{
    Material _material;

    public MixBufferPass(Material material)
      => _material = material;

    public override void RecordRenderGraph
      (RenderGraph graph, ContextContainer context)
    {
        // Not supported: Back buffer source
        var resource = context.Get<UniversalResourceData>();
        if (resource.isActiveTargetBackBuffer) return;

        // MixBufferController component reference
        var camera = context.Get<UniversalCameraData>().camera;
        var ctrl = camera.GetComponent<MixBufferController>();
        if (ctrl == null || !ctrl.enabled || !ctrl.IsReady) return;

        // Source (camera texture)
        var source = resource.activeColorTexture;
        var desc = graph.GetTextureDesc(source);

        // Temp destination
        desc.name = "MixBuffer Temp";
        desc.clearBuffer = false;
        var temp = graph.CreateTexture(desc);

        // Buffer preparation
        ctrl.PrepareBuffer(desc.width, desc.height, desc.format);
        var buffer = graph.ImportTexture(ctrl.BufferTexture);

        // Blit
        var param1 = new RenderGraphUtils.BlitMaterialParameters
          (source, temp, _material, 0, ctrl.Properties);
        graph.AddBlitPass(param1, passName: "MixBuffer (composite)");

        graph.AddCopyPass(temp, buffer, passName: "MixBuffer (copy buffer)");
        graph.AddCopyPass(temp, source, passName: "MixBuffer (copy dest)");
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

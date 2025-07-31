using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

sealed class InjectorPass : ScriptableRenderPass
{
    class PassData
    {
        public Material Material;
        public InjectorController Controller;
    }

    Material _material;

    public InjectorPass(Material material)
      => _material = material;

    public override void RecordRenderGraph
      (RenderGraph graph, ContextContainer context)
    {
        // Controller component reference
        var camera = context.Get<UniversalCameraData>().camera;
        var ctrl = camera.GetComponent<InjectorController>();
        if (ctrl == null || !ctrl.enabled || !ctrl.IsReady) return;

        // Render pass building
        using var builder = graph.
          AddRasterRenderPass<PassData>("Injector", out var data);

        // Custom pass data
        data.Material = _material;
        data.Controller = ctrl;

        // Color/depth attachments
        var resource = context.Get<UniversalResourceData>();
        builder.SetRenderAttachment(resource.activeColorTexture, 0);
        builder.SetRenderAttachmentDepth
          (resource.activeDepthTexture, AccessFlags.Write);

        // Render function registration
        builder.SetRenderFunc<PassData>((data, ctx) => ExecutePass(data, ctx));
    }

    static void ExecutePass(PassData data, RasterGraphContext ctx)
      => CoreUtils.DrawFullScreen(ctx.cmd, data.Material, data.Controller.Properties);
}

public sealed class InjectorFeature : ScriptableRendererFeature
{
    [SerializeField, HideInInspector] Shader _shader = null;

    Material _material;
    InjectorPass _pass;

    public override void Create()
    {
        _material = CoreUtils.CreateEngineMaterial(_shader);
        _pass = new InjectorPass(_material);
        _pass.renderPassEvent = RenderPassEvent.AfterRenderingTransparents;
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

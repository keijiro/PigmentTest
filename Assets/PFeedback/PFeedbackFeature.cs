using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

sealed class PFeedbackCompositePass : ScriptableRenderPass
{
    Material _material;

    public PFeedbackCompositePass(Material material)
      => _material = material;

    public override void RecordRenderGraph(RenderGraph graph,
                                           ContextContainer context)
    {
        // Not supported: Back buffer source
        var resource = context.Get<UniversalResourceData>();
        if (resource.isActiveTargetBackBuffer) return;

        // PFeedbackController component reference
        var camera = context.Get<UniversalCameraData>().camera;
        var ctrl = camera.GetComponent<PFeedbackController>();
        if (ctrl == null || !ctrl.enabled || !ctrl.IsReady) return;

        // Source (camera texture)
        var source = resource.activeColorTexture;

        // Buffer preparation
        var desc = graph.GetTextureDesc(source);
        ctrl.PrepareBuffer(desc.width, desc.height, desc.format);
        var target = graph.ImportTexture(ctrl.TargetTexture);

        // Blit
        var param = new RenderGraphUtils.BlitMaterialParameters
          (source, target, _material, 0);
        graph.AddBlitPass(param, passName: "PFeedback (composite)");
    }
}

sealed class PFeedbackBlitBackPass : ScriptableRenderPass
{
    public override void RecordRenderGraph(RenderGraph graph,
                                           ContextContainer context)
    {
        // Not supported: Back buffer source
        var resource = context.Get<UniversalResourceData>();
        if (resource.isActiveTargetBackBuffer) return;

        // PFeedbackController component reference
        var camera = context.Get<UniversalCameraData>().camera;
        var ctrl = camera.GetComponent<PFeedbackController>();
        if (ctrl == null || !ctrl.enabled || !ctrl.IsReady) return;

        // Blit source
        var source = graph.ImportTexture(ctrl.TargetTexture);

        // Blit
        var mat = Blitter.GetBlitMaterial(TextureDimension.Tex2D);
        var param = new RenderGraphUtils.BlitMaterialParameters
          (source, resource.activeColorTexture, mat, 0, ctrl.Properties);
        graph.AddBlitPass(param, passName: "PFeedback (BlitBack)");
    }
}

public sealed class PFeedbackFeature : ScriptableRendererFeature
{
    [SerializeField, HideInInspector] Shader _shader = null;

    Material _material;
    PFeedbackCompositePass _compPass;
    PFeedbackBlitBackPass _blitPass;

    public override void Create()
    {
        _material = CoreUtils.CreateEngineMaterial(_shader);

        _compPass = new PFeedbackCompositePass(_material);
        _blitPass = new PFeedbackBlitBackPass();

        _compPass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
        _blitPass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer,
                                         ref RenderingData data)
    {
        if (data.cameraData.cameraType != CameraType.Game) return;

        renderer.EnqueuePass(_compPass);
        renderer.EnqueuePass(_blitPass);
    }
}

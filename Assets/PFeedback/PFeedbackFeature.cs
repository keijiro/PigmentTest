using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

sealed class PFeedbackPass : ScriptableRenderPass
{
    Material _material;

    public PFeedbackPass(Material material)
      => _material = material;

    public override void RecordRenderGraph
      (RenderGraph graph, ContextContainer context)
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
        var desc = graph.GetTextureDesc(source);

        // Temp destination
        desc.name = "PFeedback Temp";
        desc.clearBuffer = false;
        var temp = graph.CreateTexture(desc);

        // Buffer preparation
        ctrl.PrepareBuffer(desc.width, desc.height, desc.format);
        var buffer = graph.ImportTexture(ctrl.TargetTexture);

        // Blit
        var param1 = new RenderGraphUtils.BlitMaterialParameters
          (source, temp, _material, 0, ctrl.Properties);
        graph.AddBlitPass(param1, passName: "PFeedback (composite)");

        graph.AddCopyPass(temp, buffer, passName: "PFeedback (copy buffer)");
        graph.AddCopyPass(temp, source, passName: "PFeedback (copy dest)");
    }
}

public sealed class PFeedbackFeature : ScriptableRendererFeature
{
    [SerializeField, HideInInspector] Shader _shader = null;

    Material _material;
    PFeedbackPass _pass;

    public override void Create()
    {
        _material = CoreUtils.CreateEngineMaterial(_shader);
        _pass = new PFeedbackPass(_material);
        _pass.renderPassEvent = RenderPassEvent.BeforeRenderingPostProcessing;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer,
                                         ref RenderingData data)
    {
        if (data.cameraData.cameraType != CameraType.Game) return;
        renderer.EnqueuePass(_pass);
    }
}

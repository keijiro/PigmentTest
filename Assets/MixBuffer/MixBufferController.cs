using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public sealed partial class MixBufferController : MonoBehaviour
{
    #region Public members exposed for render passes

    public bool IsReady => Properties != null;

    public MaterialPropertyBlock Properties { get; private set; }

    public RTHandle BufferTexture => _buffers.GetFrameRT(0, 0);

    public void PrepareBuffer(int width, int height, GraphicsFormat format)
    {
        RTHandle Allocator(RTHandleSystem rts, int index, GraphicsFormat format)
          => rts.Alloc(Vector3.one, format, name: "MixBuffer Buffer");

        if (_buffers == null)
        {
            _buffers = new BufferedRTHandleSystem();
            _buffers.AllocBuffer(0, (rts, i) => Allocator(rts, i, format), 1);
        }

        _buffers.SwapAndSetReferenceSize(width, height);
    }

    #endregion

    #region Private members

    BufferedRTHandleSystem _buffers;

    #endregion

    #region MonoBehaviour implementation

    void OnDisable()
      => OnDestroy();

    void OnDestroy()
    {
        _buffers?.Dispose();
        _buffers = null;
    }

    void LateUpdate()
    {
        if (Properties == null) Properties = new MaterialPropertyBlock();
        if (_buffers != null) Properties.SetTexture("_BufferTex", BufferTexture);
    }

    #endregion
}

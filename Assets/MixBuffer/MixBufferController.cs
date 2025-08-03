using UnityEngine;

[ExecuteInEditMode]
public sealed partial class MixBufferController : MonoBehaviour
{
    #region Public members exposed for render passes

    public bool IsReady => Properties != null;

    public MaterialPropertyBlock Properties { get; private set; }

    #endregion

    #region MonoBehaviour implementation

    void LateUpdate()
    {
        if (Properties == null) Properties = new MaterialPropertyBlock();
    }

    #endregion
}

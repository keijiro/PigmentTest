using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public sealed partial class PFeedbackController : MonoBehaviour
{
    #region Public properties

    [field:SerializeField, HideInInspector]
    public Shader Shader { get; set; }

    [field:SerializeField, HideInInspector]
    public Texture2D Lut { get; set; }

    public Material Material => UpdateMaterial();

    #endregion

    #region MonoBehaviour implementation

    void OnDestroy()
      => CoreUtils.Destroy(_material);

    void OnDisable()
      => OnDestroy();

    void Update() {} // Just for providing the component enable switch.

    #endregion

    #region Controller implementation

    Material _material;

    public Material UpdateMaterial()
    {
        if (_material == null)
            _material = CoreUtils.CreateEngineMaterial(Shader);

        _material.SetTexture("_MixboxLUT", Lut);

        return _material;
    }

    #endregion
}

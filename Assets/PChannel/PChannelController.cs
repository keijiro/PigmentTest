using UnityEngine;
using UnityEngine.Rendering;

[ExecuteInEditMode]
public sealed partial class PChannelController : MonoBehaviour
{
    #region Public properties

    public enum MixMethod { Mixbox, SpectralJS }

    [field:SerializeField]
    public MixMethod Method { get; set; } = MixMethod.Mixbox;

    [field:SerializeField]
    public Color Color1 { get; set; } = new Color(0.988f, 0.827f, 0.000f);

    [field:SerializeField]
    public Color Color2 { get; set; } = new Color(1.000f, 0.412f, 0.000f);

    [field:SerializeField]
    public Color Color3 { get; set; } = new Color(0.000f, 0.129f, 0.522f);

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

    public int PassIndex => (int)Method;

    public Material UpdateMaterial()
    {
        if (_material == null)
            _material = CoreUtils.CreateEngineMaterial(Shader);

        _material.SetTexture("_MixboxLUT", Lut);
        _material.SetColor("_Color1", Color1);
        _material.SetColor("_Color2", Color2);
        _material.SetColor("_Color3", Color3);

        return _material;
    }

    #endregion
}

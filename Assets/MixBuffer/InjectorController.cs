using UnityEngine;
using Klak.TestTools;

public sealed class InjectorController : MonoBehaviour
{
    [field:SerializeField] public float Interval { get; set; } = 1;
    [field:SerializeField] public Color BrushColor { get; set; } = Color.white;

    [SerializeField] ImageSource _source = null;

    public bool IsReady { get; private set; }

    public MaterialPropertyBlock Properties { get; private set; }

    async void Start()
    {
        Properties = new MaterialPropertyBlock();

        while (true)
        {
            Properties.SetColor("_BrushColor", BrushColor);
            Properties.SetTexture("_MainTex", _source.AsTexture);
            IsReady = true;
            await Awaitable.NextFrameAsync();
            IsReady = false;
            await Awaitable.WaitForSecondsAsync(Interval);
        }
    }
}

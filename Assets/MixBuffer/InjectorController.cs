using UnityEngine;
using Klak.TestTools;

public sealed class InjectorController : MonoBehaviour
{
    [field:SerializeField] public float Interval { get; set; } = 1;

    [SerializeField] ImageSource _source = null;
    [SerializeField] Color[] _palette = null;

    public bool IsReady { get; private set; }

    public MaterialPropertyBlock Properties { get; private set; }

    async void Start()
    {
        Properties = new MaterialPropertyBlock();

        while (true)
        {
            Properties.SetColor("_Color1", _palette[0]);
            Properties.SetColor("_Color2", _palette[1]);
            Properties.SetColor("_Color3", _palette[2]);
            Properties.SetTexture("_MainTex", _source.AsTexture);
            IsReady = true;
            await Awaitable.NextFrameAsync();
            IsReady = false;
            await Awaitable.WaitForSecondsAsync(Interval);
        }
    }
}

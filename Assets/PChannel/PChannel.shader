Shader "Hidden/PChannel"
{
HLSLINCLUDE

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

TEXTURE2D(_MixboxLUT);
SAMPLER(sampler_MixboxLUT);

#include "Packages/com.scrtwpns.mixbox/ShaderLibrary/Mixbox.hlsl"

#include "Packages/jp.keijiro.spectral-js-unity/Shaders/SpectralUnity.hlsl"

float4 Fragment(float4 position : SV_Position) : SV_Target0
{
    const float3 c1 = float3(0.988, 0.827, 0.000);
    const float3 c2 = float3(1.000, 0.412, 0.000);
    const float3 c3 = float3(0.000, 0.129, 0.522);

    float3 i = saturate(LOAD_TEXTURE2D(_BlitTexture, position.xy).rgb);

#if 0

    MixboxLatent z1 = MixboxRGBToLatent(c1);
    MixboxLatent z2 = MixboxRGBToLatent(c2);
    MixboxLatent z3 = MixboxRGBToLatent(c3);

    MixboxLatent z = (z1 * i.r + z2 * i.g + z3 * i.b) / (i.r + i.g + i.b + 0.01);

    float3 rgb = MixboxLatentToRGB(z);

    return float4(SRGBToLinear(rgb), 1);

#else

    float mask = saturate(dot(i, 1) * 100);
    float3 z1 = SRGBToLinear(c1);
    float3 z2 = SRGBToLinear(c2);
    float3 z3 = SRGBToLinear(c3);
    float3 rgb = SpectralMix(z1, i.r, z2, i.g, z3, i.b);

    return float4(lerp(1, rgb, mask), 1);

#endif
}

ENDHLSL

    SubShader
    {
        ZTest Off ZWrite Off Cull Off Blend Off
        Pass
        {
            Name "Pigment"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Fragment
            ENDHLSL
        }
    }
}

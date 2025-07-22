Shader "Hidden/Pigment"
{
HLSLINCLUDE

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

TEXTURE2D(_MixboxLUT);
SAMPLER(sampler_MixboxLUT);

#include "../Mixbox/ShaderLibrary/Mixbox.hlsl"

float4 Fragment(float4 position : SV_Position) : SV_Target0
{
    MixboxLatent z1 = MixboxRGBToLatent(float3(1.000, 0.153, 0.008));
    MixboxLatent z2 = MixboxRGBToLatent(float3(0.000, 0.235, 0.196));
    MixboxLatent z3 = MixboxRGBToLatent(float3(0.000, 0.129, 0.522));

    float3 i = LOAD_TEXTURE2D(_BlitTexture, position.xy).rgb;
    //i = LinearToSRGB(i);
    i *= 3;

    MixboxLatent z = (z1 * i.r + z2 * i.g + z3 * i.b) / (i.r + i.g + i.b + 0.01);

    float3 rgb = MixboxLatentToRGB(z);

    return float4(SRGBToLinear(rgb), 1);
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

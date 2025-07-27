Shader "Hidden/PChannel"
{
HLSLINCLUDE

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

TEXTURE2D(_MixboxLUT);
SAMPLER(sampler_MixboxLUT);

#include "Packages/com.scrtwpns.mixbox/ShaderLibrary/Mixbox.hlsl"

#include "Packages/jp.keijiro.spectral-js-unity/Shaders/SpectralUnity.hlsl"

float3 _Color1;
float3 _Color2;
float3 _Color3;

float4 FragmentMixbox(float4 position : SV_Position) : SV_Target0
{
    float3 i = saturate(LOAD_TEXTURE2D(_BlitTexture, position.xy).rgb);

    MixboxLatent z1 = MixboxRGBToLatent(_Color1);
    MixboxLatent z2 = MixboxRGBToLatent(_Color2);
    MixboxLatent z3 = MixboxRGBToLatent(_Color3);

    MixboxLatent z = (z1 * i.r + z2 * i.g + z3 * i.b) / (i.r + i.g + i.b + 0.01);

    float3 rgb = MixboxLatentToRGB(z);
    return float4(SRGBToLinear(rgb), 1);
}

float4 FragmentSpectralJS(float4 position : SV_Position) : SV_Target0
{
    float3 i = saturate(LOAD_TEXTURE2D(_BlitTexture, position.xy).rgb);

    float3 c1 = SRGBToLinear(_Color1);
    float3 c2 = SRGBToLinear(_Color2);
    float3 c3 = SRGBToLinear(_Color3);
    float3 rgb = SpectralMix(c1, i.r, c2, i.g, c3, i.b);

    float mask = saturate(dot(i, 1) * 100);
    return float4(lerp(1, rgb, mask), 1);
}

ENDHLSL

    SubShader
    {
        ZTest Off ZWrite Off Cull Off Blend Off
        Pass
        {
            Name "Mixbox"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragmentMixbox
            ENDHLSL
        }
        Pass
        {
            Name "SpectralJS"
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment FragmentSpectralJS
            ENDHLSL
        }
    }
}

Shader "Hidden/PChannel"
{
HLSLINCLUDE

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

TEXTURE2D(_MixboxLUT);
SAMPLER(sampler_MixboxLUT);

#include "Packages/com.scrtwpns.mixbox/ShaderLibrary/Mixbox.hlsl"

#include "Packages/jp.keijiro.spectral-js-unity/Shaders/SpectralUnity.hlsl"

static const float3 ChannelColor1 = float3(0.988, 0.827, 0.000);
static const float3 ChannelColor2 = float3(1.000, 0.412, 0.000);
static const float3 ChannelColor3 = float3(0.000, 0.129, 0.522);

float4 FragmentMixbox(float4 position : SV_Position) : SV_Target0
{
    float3 i = saturate(LOAD_TEXTURE2D(_BlitTexture, position.xy).rgb);

    MixboxLatent z1 = MixboxRGBToLatent(ChannelColor1);
    MixboxLatent z2 = MixboxRGBToLatent(ChannelColor2);
    MixboxLatent z3 = MixboxRGBToLatent(ChannelColor3);

    MixboxLatent z = (z1 * i.r + z2 * i.g + z3 * i.b) / (i.r + i.g + i.b + 0.01);

    float3 rgb = MixboxLatentToRGB(z);
    return float4(SRGBToLinear(rgb), 1);
}

float4 FragmentSpectralJS(float4 position : SV_Position) : SV_Target0
{
    float3 i = saturate(LOAD_TEXTURE2D(_BlitTexture, position.xy).rgb);

    float3 c1 = SRGBToLinear(ChannelColor1);
    float3 c2 = SRGBToLinear(ChannelColor2);
    float3 c3 = SRGBToLinear(ChannelColor3);
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

Shader "Hidden/PFeedback"
{
HLSLINCLUDE

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
#include "Packages/jp.keijiro.spectral-js-unity/Shaders/SpectralUnity.hlsl"

Texture2D _HistoryTex;

float4 Frag(float4 position : SV_Position) : SV_Target0
{
    float3 hist = saturate(LOAD_TEXTURE2D(_HistoryTex, position.xy).rgb);
    float3 draw = saturate(LOAD_TEXTURE2D(_BlitTexture, position.xy).rgb);

    float hist_pow = saturate(dot(hist, 10));
    float draw_pow = saturate(dot(draw, 10));

    float3 c = SpectralMix(hist, hist_pow * 3, draw, draw_pow, (float3)1, 0.1);

    return float4(c, 1);
}

ENDHLSL

    SubShader
    {
        ZTest Off ZWrite Off Cull Off Blend Off
        Pass
        {
            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            ENDHLSL
        }
    }
}

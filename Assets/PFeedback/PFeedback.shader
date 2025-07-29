Shader "Hidden/PFeedback"
{
HLSLINCLUDE

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
#include "Packages/jp.keijiro.spectral-js-unity/Shaders/SpectralUnity.hlsl"

Texture2D _HistoryTex;

float4 Frag(float4 position : SV_Position) : SV_Target0
{
    float3 i = saturate(LOAD_TEXTURE2D(_BlitTexture, position.xy).rgb);
    float3 h = saturate(LOAD_TEXTURE2D(_HistoryTex, position.xy).rgb);

    return float4(i + h * 0.9, 1);
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

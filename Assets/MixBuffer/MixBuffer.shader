Shader "Hidden/MixBuffer"
{
HLSLINCLUDE

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"
#include "Packages/jp.keijiro.spectral-js-unity/Shaders/SpectralUnity.hlsl"

Texture2D _BufferTex;

float4 Frag(float4 position : SV_Position) : SV_Target0
{
    float4 c0 = saturate(LOAD_TEXTURE2D(_BufferTex, position.xy));
    float4 c1 = saturate(LOAD_TEXTURE2D(_BlitTexture, position.xy));
    float3 c = SpectralMix(c0.rgb, c0.a, c1.rgb / LinearToSRGB(c1.a), c1.a);
    return float4(c, max(c0.a, c1.a));
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

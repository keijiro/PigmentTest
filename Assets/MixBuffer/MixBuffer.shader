Shader "Hidden/MixBuffer"
{
HLSLINCLUDE

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#include "Packages/jp.keijiro.spectral-js-unity/Shaders/SpectralUnity.hlsl"

Texture2D _MainTex;
sampler2D _BufferTex;

#define ANGLES 7

float2 cos_sin(float x) { return float2(cos(x), sin(x)); }
float2 rot90(float2 v) { return v.yx * float2(1, -1); }

float outflow(float2 uv, float l, sampler2D buffer)
{
    float acc = 0;
    for (int i = 0; i < ANGLES; i++)
    {
        float2 dir = cos_sin(PI * 2 / ANGLES * i);
        acc += dot(dir, LinearToSRGB(tex2D(buffer, uv + dir * l)).xy - 0.5);
    }
    return acc / ANGLES;
}

void Vert(uint vertexID : SV_VertexID,
          out float4 outPosition : SV_Position,
          out float2 outTexCoord : TEXCOORD0)
{
    outPosition = GetFullScreenTriangleVertexPosition(vertexID);
    outTexCoord = GetFullScreenTriangleTexCoord(vertexID);
}

float4 Frag(float4 position : SV_Position,
            float2 texCoord : TEXCOORD0) : SV_Target
{
    const float delta = 1.0 / 640;
    float2 acc = float2(0.0, 0.0);
    float l = delta;

    for (int i = 0; i < 8; i++)
    {
        for (int j = 0; j < ANGLES; j++)
        {
            float2 dir = cos_sin(PI * 2 / ANGLES * j);
            acc += rot90(dir) * outflow(texCoord + dir * l, l, _BufferTex);
        }
        l *= 2;
    }

    acc *= 4;

    float4 c0 = tex2D(_BufferTex, texCoord + acc * delta / ANGLES);

    float4 c1 = saturate(LOAD_TEXTURE2D(_MainTex, position.xy));
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

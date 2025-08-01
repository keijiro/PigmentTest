Shader "Hidden/Injector"
{
HLSLINCLUDE

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

sampler2D _ColorMap;
sampler2D _DepthMap;
float4 _InverseProjection;
float4x4 _InverseView;

float4 _Color1;
float4 _Color2;
float4 _Color3;
sampler2D _MainTex;

void Vertex(uint vertexID : VERTEXID_SEMANTIC,
            out float4 outPosition : SV_Position,
            out float2 outTexCoord : TEXCOORD0)
{
    outPosition = GetFullScreenTriangleVertexPosition(vertexID);
    outTexCoord = GetFullScreenTriangleTexCoord(vertexID);
}

float4 Fragment(float4 position : SV_Position,
                float2 texCoord : TEXCOORD) : SV_Target
{
    float3 rgb = tex2D(_MainTex, texCoord).rgb;
    float alpha = dot(LinearToSRGB(rgb), 0.33333);
    return alpha < 0.2 ? 0 :
           (alpha < 0.5 ? _Color1 :
           (alpha < 0.75 ? _Color2 : _Color3));
}

ENDHLSL

    SubShader
    {
        Pass
        {
            ZTest Always ZWrite Off Cull Off Blend Off
            HLSLPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            ENDHLSL
        }
    }
    Fallback Off
}

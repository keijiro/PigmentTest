Shader "Hidden/Injector"
{
HLSLINCLUDE

#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"

sampler2D _ColorMap;
sampler2D _DepthMap;
float4 _InverseProjection;
float4x4 _InverseView;

float4 _BrushColor;
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
    float alpha = dot(tex2D(_MainTex, texCoord).rgb, 0.3);
    return float4(_BrushColor.rgb, alpha * 4);
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

Shader "Custom/DialogueBubble"
{
    Properties
    {
        _MainTex ("Main Tex", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _CircleRadius ("Circle Radius", Range(0.0, 1.0)) = 0.5
        _EdgeSoftess ("Edge Softness", Range(0.0, 0.5)) = 0.05
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_ST;

            float4 _BaseColor;

            float _CircleRadius;
            float _EdgeSoftess;

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            half4 frag (Varyings IN) : SV_Target
            {
                float2 centeredUV = IN.uv - 0.5;
                float dist = length(centeredUV);

                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, centeredUV);
                col *= _BaseColor;

                float circleMask = smoothstep(_CircleRadius, _CircleRadius - _EdgeSoftess, dist);
                col.a *= circleMask;

                return col;
            }
            ENDHLSL
        }
    }
}

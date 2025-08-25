Shader "Custom/Minimap"
{
    Properties
    {
        _MainTex ("Main Tex", 2D) = "white" {}
        _CircleRadius ("Circle Radius", Range(0.0, 1.0)) = 0.5
        _EdgeSoftess ("Edge Softness", Range(0.0, 0.5)) = 0.05
        _OutlineThickness ("Outline Thickness", Range(0.0, 0.5)) = 0.04
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _VignetteIntensity ("Vignette Intensity", Range(0.0, 1.0)) = 0.5
        _VignetteColor ("Vignette Color", Color) = (0,0,0,1)
        _DistortionStrength ("Distortion Strength", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100

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

            float _CircleRadius;
            float _EdgeSoftess;

            float4 _OutlineColor;
            float4 _OutlineThickness;

            float _VignetteIntensity;
            float4 _VignetteColor;

            float _DistortionStrength;

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

                float2 barrelUV = centeredUV * (1.0 + _DistortionStrength * dot(centeredUV, centeredUV));
                barrelUV += 0.5;

                half4 col = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, barrelUV);

                float circleMask = smoothstep(_CircleRadius, _CircleRadius - _EdgeSoftess, dist);
                col.a *= circleMask;

                float vignette = smoothstep(0.7, _CircleRadius, dist);
                col.rgb = lerp(col.rgb, _VignetteColor.rgb, vignette * _VignetteIntensity);
                
                float outline = smoothstep(_CircleRadius - 2 * _OutlineThickness, _CircleRadius - _OutlineThickness, dist);
                col.rgb = lerp(col.rgb, _OutlineColor.rgb, outline);

                return col;
            }
            ENDHLSL
        }
    }
}

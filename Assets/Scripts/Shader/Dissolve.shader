Shader "Custom/Dissolve"
{
    Properties
    {
        _MainTex ("Albedo Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        [HDR] _EdgeColor ("Edge Color", Color) = (1, 0.5, 0, 1)
        _EdgeThickness ("Edge Thickness", Range(0.0, 0.5)) = 0.05
        _DissolveAmount ("Dissolve Amount", Range(0.0, 1.0)) = 0.0
    }

    SubShader
    {
        Tags { "RenderType"="TransparentCutout" "RenderPipeline"="UniversalPipeline" "Queue"="AlphaTest" }
        LOD 100

        Pass
        {
            Name "Unlit"
            
            ZWrite On
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float2 uv           : TEXCOORD0;
                float4 positionHCS  : SV_POSITION;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            TEXTURE2D(_NoiseTex);
            SAMPLER(sampler_NoiseTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _EdgeColor;
                float _EdgeThickness;
                float _DissolveAmount;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half noiseVal = SAMPLE_TEXTURE2D(_NoiseTex, sampler_NoiseTex, IN.uv).r;
                clip(noiseVal - _DissolveAmount);

                half4 albedo = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                half3 finalRGB = albedo.rgb;

                if(_DissolveAmount > 0.001 && noiseVal < _DissolveAmount + _EdgeThickness)
                {
                    finalRGB += _EdgeColor.rgb;
                }

                return half4(finalRGB, 1.0);
            }
            ENDHLSL
        }
    }
}
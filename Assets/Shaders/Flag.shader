Shader "Custom/Flag"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}

    	_Speed("Wind Speed", Range(0, 20)) = 5.0
    	_Frequency("Wave Frequency", Range(0, 10)) = 2.0
    	_Amplitude("Wave Amplitude", Range(0, 2)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }
        Cull Off

        Pass
        {
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
                float highlight : DISTORTION;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                float _Speed;
                float _Frequency;
                float _Amplitude;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                float wave = sin(IN.positionOS.x * _Frequency + _Time.y * _Speed);
                wave = wave * _Amplitude * IN.uv.x;
                IN.positionOS.y += wave * _Amplitude * IN.uv.x;

                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.highlight = abs(wave) * 0.5;

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 base_color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
                half4 color = half4(0.7 * base_color.rgb, base_color.a);
                color += (1.0 + IN.highlight) * base_color;
                return color;
            }
            ENDHLSL
        }
    }
}

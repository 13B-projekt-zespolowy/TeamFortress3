Shader "Custom/Flag_Turbulence_And_Erosion_Final"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}

        [Header(Wind Settings)]
        _Speed("Wind Speed", Range(0, 20)) = 5.0
        _Frequency("Wave Frequency", Range(0, 10)) = 2.0
        _Amplitude("Wave Amplitude", Range(0, 2)) = 0.5
        _FlagRotation("Wind Direction", Range(0, 360)) = 0.0

        [Header(Gravity and Weight)]
        _Gravity("Gravity Strength", Range(0, 2)) = 0.3
        _GravitySag("Gravity Sag Curve", Range(1, 5)) = 2.0

        [Header(Micro Turbulence)]
        _TurbulenceSpeed("Turbulence Speed", Range(0, 50)) = 25.0
        _TurbulenceFreq("Turbulence Frequency", Range(0, 30)) = 12.0
        _TurbulenceAmp("Turbulence Amplitude", Range(0, 0.5)) = 0.05

        [Header(Erosion and Tatters)]
        _Erosion("Erosion Scale", Range(0, 1)) = 0.3
        _ErosionNoiseScale("Noise Scale", Range(1, 50)) = 20.0

        [Header(Shading and Highlights)]
        _HighlightIntensity("Highlight Intensity", Range(0, 2)) = 0.3
        _ShadowIntensity("Shadow Intensity", Range(0, 1)) = 0.5
        _HighlightSharpness("Highlight Sharpness", Range(1, 10)) = 2.0
        _HighlightTint("Highlight Color", Color) = (1, 1, 1, 1)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" "Queue" = "AlphaTest" }
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
                float highlight : TEXCOORD1;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                float _Speed;
                float _Frequency;
                float _Amplitude;
                float _FlagRotation;

                float _Gravity;
                float _GravitySag;
                
                float _TurbulenceSpeed;
                float _TurbulenceFreq;
                float _TurbulenceAmp;

                float _Erosion;
                float _ErosionNoiseScale;

                half _HighlightIntensity;
                half _ShadowIntensity;
                float _HighlightSharpness;
                half4 _HighlightTint;
            CBUFFER_END

            float safe_pow(float baseValue, float expValue)
            {
                return pow(max(0.0001, baseValue), expValue);
            }

            float hash(float2 p)
            {
                return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123);
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);
                float2 u = f * f * (3.0 - 2.0 * f);
                return lerp(lerp(hash(i + float2(0.0,0.0)), hash(i + float2(1.0,0.0)), u.x),
                            lerp(hash(i + float2(0.0,1.0)), hash(i + float2(1.0,1.0)), u.x), u.y);
            }

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float pinWeight = IN.uv.x; 
                float rad = _FlagRotation * 0.0174532925;
                float s, c;
                sincos(rad, s, c);

                float rawWaveSin = sin(IN.positionOS.x * _Frequency + _Time.y * _Speed);
                float mainWave = rawWaveSin * _Amplitude;
                float microWave = sin(IN.positionOS.x * _TurbulenceFreq + _Time.y * _TurbulenceSpeed) * _TurbulenceAmp * pinWeight;
                float totalWave = (mainWave + microWave) * pinWeight;

                float3 localPos = IN.positionOS.xyz;
                float finalX = localPos.x * c - localPos.y * s - s * totalWave;
                float finalY = localPos.x * s + localPos.y * c + c * totalWave;
                localPos.x = finalX;
                localPos.y = finalY;

                float3 worldPos = TransformObjectToWorld(localPos);

                worldPos.y -= safe_pow(pinWeight, _GravitySag) * _Gravity;

                OUT.positionHCS = TransformWorldToHClip(worldPos);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.highlight = rawWaveSin * pinWeight; 

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float n = noise(IN.uv * _ErosionNoiseScale);
                clip(n - (IN.uv.x * _Erosion));

                half4 base_color = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
                float rawWave = IN.highlight; 

                float shadowMask = max(0.0, -rawWave);
                shadowMask = safe_pow(shadowMask, _HighlightSharpness);
                float3 shadowEffect = base_color.rgb * (1.0 - (shadowMask * _ShadowIntensity));

                float highlightMask = max(0.0, rawWave);
                highlightMask = safe_pow(highlightMask, _HighlightSharpness);
                float3 highlightEffect = _HighlightTint.rgb * _HighlightIntensity * highlightMask;

                base_color.rgb = shadowEffect + highlightEffect;

                return base_color;
            }
            ENDHLSL
        }
    }
}
Shader "Custom/URP/FountainWater"
{
    Properties
    {
        _WaterColor ("Water Color", Color) = (0.15,0.55,0.75,0.55)
        _RefractionStrength ("Refraction Strength", Range(0,1)) = 5
        _ColorModulation ("Color Modulation", Range(0,2)) = 0.4
        _WaveSpeed ("Wave Speed", Float) = 2.0
        _WaveScale ("Wave Scale", Float) = 22.0
        _WaveAmplitude ("Wave Amplitude", Range(0, 2)) = 1
        _NoiseScale ("Noise Scale", Float) = 6.0
        _FlowSpeed ("Waterfall Flow Speed", Float) = 3.0
        _Center ("Wave Center", Vector) = (0.5,0.5,0,0)
        _FresnelPower ("Fresnel Power", Range(0.1,8)) = 5.0
        _NormalStrength ("Normal Strength", Range(0,4)) = 1.5
        _SpecularStrength ("Specular Strength", Range(0,8)) = 2.0
        _SpecSmoothness ("Specular Smoothness", Range(1,512)) = 512
        _ReflSmoothnes ("Sky Reflection Smoothness", float) = 1
        _ChromAb ("Chromatic Aberration", Range(0,1)) = 0.0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalRenderPipeline"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }

        Pass
        {
            Name "Forward"

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature_local_fragment _ENVIRONMENTREFLECTIONS_OFF
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BLENDING
            #pragma multi_compile_fragment _ _REFLECTION_PROBE_BOX_PROJECTION

            #if (UNITY_VERSION >= 60010000)
                #pragma multi_compile _ _CLUSTER_LIGHT_LOOP
                #pragma multi_compile_fragment _ _REFLECTION_PROBE_ATLAS
            #endif

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/GlobalIllumination.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
            };

            struct Varyings
            {
                float4 positionCS  : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 worldPos    : TEXCOORD1;
                float3 normalWS    : TEXCOORD2;
                float3 tangentWS   : TEXCOORD3;
                float3 bitangentWS : TEXCOORD4;
                float4 screenPos   : TEXCOORD5;
            };

            TEXTURE2D_X(_CameraOpaqueTexture);
            SAMPLER(sampler_CameraOpaqueTexture);

            CBUFFER_START(UnityPerMaterial)
                float4 _WaterColor;
                float _RefractionStrength;
                float _ColorModulation;
                float _WaveSpeed;
                float _WaveScale;
                float _WaveAmplitude;
                float _NoiseScale;
                float _FlowSpeed;
                float4 _Center;
                float _FresnelPower;
                float _NormalStrength;
                float _SpecularStrength;
                float _SpecSmoothness;
                float _ReflSmoothness;
                float _ChromAb;
                float _IsTop;
            CBUFFER_END

            // =========================
            // NOISE
            // =========================

            float hash(float2 p)
            {
                p = frac(p * 0.3183099 + 0.1);
                p *= 17.0;
                return frac(p.x * p.y * (p.x + p.y));
            }

            float noise(float2 p)
            {
                float2 i = floor(p);
                float2 f = frac(p);

                float a = hash(i);
                float b = hash(i + float2(1,0));
                float c = hash(i + float2(0,1));
                float d = hash(i + float2(1,1));

                float2 u = f * f * (3.0 - 2.0 * f);

                return lerp(a,b,u.x)
                     + (c-a)*u.y*(1.0-u.x)
                     + (d-b)*u.x*u.y;
            }

            float fbm(float2 p)
            {
                float v = 0;
                float a = 0.5;
                for(int i = 0; i < 5; i++)
                {
                    v += noise(p) * a;
                    p *= 2;
                    a *= 0.5;
                }
                return v;
            }

            // =========================
            // TOP WATER
            // =========================

            float heightTop(float2 uv)
            {
                float ripple = sin(length(uv - _Center.xy) * _WaveScale - _Time.y * _WaveSpeed) * _WaveAmplitude;
                float n = fbm(uv * _NoiseScale + _Time.y * 0.15);
                return ripple * n;
            }

            // =========================
            // WATERFALL
            // =========================

            float waterfall(float2 uv)
            {
                float2 flowUV = uv * _NoiseScale;
                flowUV.y -= _Time.y * -_FlowSpeed;

                float n = fbm(flowUV);

                float streak = noise(float2(
                    uv.x * 20,
                    uv.y * 2 + _Time.y * _FlowSpeed
                ));

                return lerp(n, streak, 0.6);
            }

            // =========================
            // VERTEX
            // =========================

            Varyings vert (Attributes IN)
            {
                Varyings OUT;

                VertexPositionInputs vp = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs vn = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);

                OUT.positionCS  = vp.positionCS;
                OUT.worldPos    = vp.positionWS;
                OUT.normalWS    = vn.normalWS;
                OUT.tangentWS   = vn.tangentWS;
                OUT.bitangentWS = vn.bitangentWS;
                OUT.uv          = IN.uv;
                OUT.screenPos   = ComputeScreenPos(vp.positionCS);

                return OUT;
            }

            // =========================
            // NORMAL
            // =========================

float3 calcNormal(float2 uv, bool isTop)
{
    float eps = 0.01;
    float h1, h2, h3, h4;

    if (isTop)
    {
        h1 = heightTop(uv + float2(eps, 0));
        h2 = heightTop(uv - float2(eps, 0));
        h3 = heightTop(uv + float2(0, eps));
        h4 = heightTop(uv - float2(0, eps));
    }
    else
    {
        h1 = waterfall(uv + float2(eps, 0));
        h2 = waterfall(uv - float2(eps, 0));
        h3 = waterfall(uv + float2(0, eps));
        h4 = waterfall(uv - float2(0, eps));
    }

    float dx = h1 - h2;
    float dy = h3 - h4;
    if (isTop)
    {
        return normalize(float3(-dx * _NormalStrength, -dy * _NormalStrength, 1));
    }
    else
    {
        return normalize(float3(-dx * _NormalStrength, 1, -dy * _NormalStrength));
    }

}

            // =========================
            // FRAGMENT
            // =========================

            half4 frag (Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;


                bool isTop = IN.normalWS.y > 0.5;

                float3 nTS = calcNormal(uv, isTop);

                // Correct TBN transform: tangent-space -> world-space
                float3 normalWS = normalize(
                    nTS.x * normalize(IN.tangentWS) +
                    nTS.y * normalize(IN.bitangentWS) +
                    nTS.z * normalize(IN.normalWS)
                );

                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;

                float2 distortion = normalWS.xz * _RefractionStrength;
                float2 aspect = float2(_ScreenParams.x / _ScreenParams.y, 1);
                distortion *= aspect;

                float2 uvR = screenUV + distortion + normalWS.xz * _ChromAb;
                float2 uvG = screenUV + distortion;
                float2 uvB = screenUV + distortion - normalWS.xz * _ChromAb;

                float r = SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uvR).r;
                float g = SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uvG).g;
                float b = SAMPLE_TEXTURE2D_X(_CameraOpaqueTexture, sampler_CameraOpaqueTexture, uvB).b;

                float4 refracted = float4(r, g, b, 1);

                Light mainLight = GetMainLight();

                float3 L = normalize(mainLight.direction);
                float3 V = normalize(_WorldSpaceCameraPos - IN.worldPos);

                float NdotL = saturate(dot(normalWS, L));

                float3 H = normalize(L + V);
                float spec = pow(saturate(dot(normalWS, H)), _SpecSmoothness) * _SpecularStrength;

                float fresnel = pow(1.0 - saturate(dot(V, normalWS)), _FresnelPower);

                float3 R = reflect(-V, normalWS);


                float3 skyReflection = GlossyEnvironmentReflection(
                    R,
                    IN.worldPos.xyz,
                    _ReflSmoothness,
                    1,
                    screenUV
                ).rgb;

                float pattern = isTop ? heightTop(uv) : waterfall(uv);

                float3 baseCol = lerp(refracted.rgb, _WaterColor.rgb, 0.35);

                float3 reflectionCol = skyReflection * fresnel;

                float3 col = lerp(baseCol, reflectionCol, fresnel * 0.75);

                col += NdotL * 0.12 * mainLight.color;
                col += spec * mainLight.color;
                col += pattern * _ColorModulation * 0.1;

                return float4(col, _WaterColor.a);
            }

            ENDHLSL
        }
    }
}
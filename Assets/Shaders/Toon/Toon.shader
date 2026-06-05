Shader "Custom/Toon"
{
    Properties
    {
        [MainColor] _BaseColor("Base Color", Color) = (1, 1, 1, 1)
        [MainTexture] _BaseMap("Base Map", 2D) = "white" {}
        _ShadowColor("Shadow Color", Color) = (0.2, 0.2, 0.3, 1)
        _ShadowSize("Shadow Size", Range(0.0, 1.0)) = 0.5
        _ShadowBlend("Shadow Blend", Range(0.0, 1.0)) = 0.0
        _SpecularColor("Specular Color", Color) = (1, 1, 1, 1)
        _SpecularSize("Specular Size", Range(0.0, 1.0)) = 0.2
        _OutlineColor("Outline Color", Color) = (0.3, 0.3, 0.3, 1)
        _OutlineBlend("Outline Blend", Range(0.0, 1.0)) = 0.0
        _OutlineWidth("Outline Width", Range(0.0, 0.01)) = 0.005
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                half4 _ShadowColor;
                float _ShadowSize;
                float _ShadowBlend;
                half4 _SpecularColor;
                float _SpecularSize;
                half4 _OutlineColor;
                float _OutlineWidth;
                float _OutlineBlend;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;

                Light light = GetMainLight();
                float3 lightDir = normalize(light.direction);
                float3 normal = normalize(IN.normalWS);
                float3 viewDir = normalize(GetWorldSpaceViewDir(IN.positionWS));

                float NdotL = dot(normal, lightDir) * 0.5 + 0.5;
                float shadowMask = step(1.0 - _ShadowSize, NdotL);
                half4 shadowColorFinal = lerp(_ShadowColor, tex * _ShadowColor, _ShadowBlend);
                half4 diffuse = tex * lerp(shadowColorFinal, half4(1,1,1,1), shadowMask);

                float3 halfDir = normalize(lightDir + viewDir);
                float NdotH = dot(normal, halfDir);
                float specMask = step(1.0 - _SpecularSize, NdotH);
                diffuse += _SpecularColor * specMask;

                return diffuse;
            }
            ENDHLSL
        }

        Pass
        {
            Name "Outline"
            Cull Front

            HLSLPROGRAM
            #pragma vertex vertOutline
            #pragma fragment fragOutline

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                float4 _BaseMap_ST;
                half4 _ShadowColor;
                float _ShadowSize;
                float _ShadowBlend;
                half4 _SpecularColor;
                float _SpecularSize;
                half4 _OutlineColor;
                float _OutlineWidth;
                float _OutlineBlend;
            CBUFFER_END

            Varyings vertOutline(Attributes IN)
            {
                Varyings OUT;
                float3 normalWS = TransformObjectToWorldNormal(IN.normalOS);
                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                positionWS += normalWS * _OutlineWidth;
                OUT.positionHCS = TransformWorldToHClip(positionWS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                return OUT;
            }

            half4 fragOutline(Varyings IN) : SV_Target
            {
                half4 tex = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor * 0.3;
                return lerp(_OutlineColor, tex, _OutlineBlend);
            }
            ENDHLSL
        }
    }
}
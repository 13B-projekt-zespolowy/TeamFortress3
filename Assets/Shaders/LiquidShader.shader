Shader "Custom/URP/Liquid"
{
    Properties
    {
        // ----------------------------- MAIN -----------------------------
        [Header(Main)]
        _LiquidColorTop ("Liquid Color Top", Color) = (1, 1, 1, 1)
        _LiquidColorBottom ("Liquid Color Bottom", Color) = (1, 1, 1, 1)
        _SurfaceColor ("Surface Color", Color) = (1, 1, 1, 1)

        _FillAmount ("Fill Amount", Range(0,1)) = 0
        _FillMinY ("Container Bottom (Local Y)", Float) = -1.0
        _FillMaxY ("Container Top (Local Y)", Float) = 1.0

        _GradientSpread ("Gradient Spread", Range(0.01, 2)) = 2.0
        _GradientOffset ("Gradient Offset", Range(-1, 1)) = 0

        // ----------------------------- REFRACTION -----------------------------
        [Header(Refraction)]
        _RefractionStrength ("Refraction Strength", Range(0, 0.2)) = 0.05

        // ----------------------------- WOBBLE -----------------------------
        [Header(Wobble)]
        _WobbleX ("Wobble X", Float) = 0
        _WobbleZ ("Wobble Z", Float) = 0
        _WobbleFreq ("Wobble Frequency", Float) = 4
        _WobbleAmpl ("Wobble Amplitude", Range(0, 0.1)) = 0.025

        // ----------------------------- FOAM -----------------------------
        [Header(Foam)]
        _FoamColor ("Foam Color", Color) = (1, 1, 1, 1)
        _FoamWidth ("Foam Width", Range(0,0.2)) = 0.04
        _BackFoamWidth ("Back Foam Width", Range(0,0.2)) = 0.04
        _FoamSmoothness ("Foam Smoothness", Range(0.001, 1)) = 0.5

        // ----------------------------- FRESNEL -----------------------------
        [Header(Fresnel)]
        _FresnelColor ("Fresnel Color", Color) = (0.3, 0.8, 1.0, 1)
        _FresnelPower ("Fresnel Power", Float) = 5
        _FresnelStrength ("Fresnel Strength", Float) = 1

        // ----------------------------- RIPPLES -----------------------------
        [Header(Ripples)]
        _RippleDensity ("Ripples Density", Float) = 2
        _RippleSpeed ("Ripples Speed", Float) = 0.03
        _RippleBrightness ("Ripples Brightness", Range(0,5)) = 0.5

        // ----------------------------- TRANSPARENCY -----------------------------
        [Header(Transparency)]
        _Alpha ("Alpha", Range(0,1)) = 0.8
        _AlphaFadeStart ("Fade Start Distance", Float) = 3.0
        _AlphaFadeEnd ("Fade End Distance", Float) = 4.0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Transparent"
            "Queue"="Transparent-1"
        }

        Pass
        {
            Name "Liquid"
            ZWrite On
            Cull Off

            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareOpaqueTexture.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS      : SV_POSITION;
                float3 worldPos        : TEXCOORD0;
                float3 localPos        : TEXCOORD1;
                float3 normalWS        : TEXCOORD2;
                float2 uv              : TEXCOORD3;
                float3 viewDirWS       : TEXCOORD4;
                float3 objectOriginWS  : TEXCOORD5; 
                float4 screenPos       : TEXCOORD6;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _LiquidColorTop;
                float4 _LiquidColorBottom;
                float _GradientSpread;
                float _GradientOffset;

                float4 _SurfaceColor;
                float4 _FoamColor;
                float4 _FresnelColor;

                float _FillAmount;
                float _FillMinY;
                float _FillMaxY;

                float _RefractionStrength;

                float _WobbleX;
                float _WobbleZ;
                float _WobbleFreq;
                float _WobbleAmpl;

                float _FoamWidth;
                float _BackFoamWidth;
                float _FoamSmoothness;
                
                float _FresnelPower;
                float _FresnelStrength;

                float _RippleDensity;
                float _RippleSpeed;
                float _RippleBrightness;

                float _Alpha;
                float _AlphaFadeStart;
                float _AlphaFadeEnd;

                float _RandomOffset;
            CBUFFER_END

            // https://docs.unity3d.com/Packages/com.unity.shadergraph@17.3/manual/Voronoi-Node.html
            inline float2 unity_voronoi_noise_randomVector (float2 UV, float offset)
            {
                float2x2 m = float2x2(15.27, 47.63, 99.41, 89.98);
                UV = frac(sin(mul(UV, m)) * 46839.32);
                return float2(sin(UV.y*+offset)*0.5+0.5, cos(UV.x*offset)*0.5+0.5);
            }

            float Unity_Voronoi_float(float2 UV, float AngleOffset, float CellDensity)
            {
                float2 g = floor(UV * CellDensity);
                float2 f = frac(UV * CellDensity);
                float3 res = float3(8.0, 0.0, 0.0);

                for(int y=-1; y<=1; y++)
                {
                    for(int x=-1; x<=1; x++)
                    {
                        float2 lattice = float2(x,y);
                        float2 offset = unity_voronoi_noise_randomVector(lattice + g, AngleOffset);
                        float d = distance(lattice + offset, f);
                        if(d < res.x)
                        {
                            res = float3(d, offset.x, offset.y);
                        }
                    }
                }

                return res.x;
            }

            Varyings vert (Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS);

                OUT.positionCS = posInputs.positionCS;
                OUT.worldPos = GetAbsolutePositionWS(posInputs.positionWS);
                OUT.localPos = IN.positionOS.xyz;

                OUT.normalWS = normalInputs.normalWS;
                OUT.uv = IN.uv;
                OUT.viewDirWS = GetWorldSpaceViewDir(posInputs.positionWS);
                OUT.objectOriginWS = TransformObjectToWorld(float3(0,0,0));
                OUT.screenPos = ComputeScreenPos(posInputs.positionCS);
                return OUT;
            }

            half4 frag (Varyings IN, FRONT_FACE_TYPE isFrontFace : FRONT_FACE_SEMANTIC) : SV_Target
            {
                // ------------------------------------------- CLIPPING PLANE -------------------------------------------
                float3 liquidNormalWS = normalize(float3(-_WobbleX, 1.0, -_WobbleZ));

                float currentLiquidFill = lerp(_FillMinY, _FillMaxY, _FillAmount);
                float3 liquidSurfaceCenterWS = IN.objectOriginWS + float3(0, currentLiquidFill, 0);

                float distanceToPlane = dot(liquidNormalWS, IN.worldPos - liquidSurfaceCenterWS);
                
                // ------------------------------------------- WOBBLE -------------------------------------------
                float wobbleIntensity = abs(_WobbleX) + abs(_WobbleZ);
                float wobble = sin((IN.localPos.x * _WobbleFreq) + (IN.localPos.z * _WobbleFreq) + (_Time.y + _RandomOffset)) * (_WobbleAmpl * wobbleIntensity);
                distanceToPlane -= wobble;

                clip(-distanceToPlane);

                float3 normalWS = normalize(IN.normalWS);
                float3 viewDir = normalize(IN.viewDirWS);

                // ------------------------------------------- TOP SURFACE DETECTION -------------------------------------------
                bool frontFace = IS_FRONT_VFACE(isFrontFace, true, false);
                float sideMask = frontFace ? 1.0 : 0.0;
                float surfaceMask = 1.0 - sideMask;

                // ------------------------------------------- GRADIENT -------------------------------------------
                float currentLiquidHeight = max(currentLiquidFill - _FillMinY, 0.0001);
                float normalizedHeight = saturate((IN.localPos.y - _FillMinY) / currentLiquidHeight);

                float gradientFactor = saturate((normalizedHeight + _GradientOffset) / max(_GradientSpread, 0.0001));
                float3 currentLiquidColor = lerp(_LiquidColorBottom.rgb, _LiquidColorTop.rgb, gradientFactor);

                // ------------------------------------------- FOAM -------------------------------------------
                float currentFoamWidth = frontFace ? _FoamWidth : _BackFoamWidth;
                float foamEdge = currentFoamWidth * (1.0 - _FoamSmoothness);
                float foam = 1.0 - smoothstep(foamEdge, currentFoamWidth, abs(distanceToPlane));

                // ------------------------------------------- FRESNEL -------------------------------------------
                float fresnel = pow(1.0 - saturate(dot(viewDir, normalWS)), _FresnelPower) * _FresnelStrength;

                // ------------------------------------------- RIPPLES -------------------------------------------
                float2 rippleUV = IN.uv;
                rippleUV.y -= _Time.y * _RippleSpeed;
                float rippleVoronoi = pow(Unity_Voronoi_float(rippleUV, _Time.y, _RippleDensity), 5);

                // ------------------------------------------- TRANSPARENCY & REFRACTION -------------------------------------------
                float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
                float distFactor = saturate((_AlphaFadeEnd - IN.screenPos.w) / (_AlphaFadeEnd - _AlphaFadeStart));
                float finalOpacity = lerp(1.0, _Alpha, distFactor);

                screenUV += normalWS.xz * _RefractionStrength * distFactor;
                float3 refractionColor = SampleSceneColor(screenUV);

                // ------------------------------------------- FINAL COLORS -------------------------------------------
                float3 sideColor = lerp(currentLiquidColor * refractionColor, currentLiquidColor, finalOpacity);
                float3 topColor = lerp(_SurfaceColor.rgb * refractionColor, _SurfaceColor.rgb, finalOpacity);
                
                float3 finalColor = lerp(sideColor, topColor, surfaceMask);

                // Fresnel, Foam and Ripples
                finalColor = lerp(finalColor, _FresnelColor.rgb, saturate(fresnel * sideMask));
                finalColor = lerp(finalColor, _FoamColor.rgb, foam);
                finalColor += rippleVoronoi * _SurfaceColor.rgb * _RippleBrightness * sideMask;

                return float4(finalColor, 1.0);
            }

            ENDHLSL
        }
    }
}
Shader "Custom/URP/PSXLit"
{
    Properties
    {
        _BaseMap("Texture", 2D) = "white" {}
        _BaseColor("Base Color", Color) = (1,1,1,1)

        _VertexSnap("Vertex Snap Strength", Float) = 160.0
        _AffineWarp("Affine Warp Strength", Float) = 0.0

        _LightIntensity("Light Intensity", Range(0, 2)) = 1.0
        _ShadowStrength("Shadow Strength", Range(0, 1)) = 1.0
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "RenderPipeline"="UniversalPipeline"
            "Queue"="Geometry"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile_fog

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv         : TEXCOORD0;
                float3 normalWS   : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                float fogCoord    : TEXCOORD3;
                float4 screenPos  : TEXCOORD4;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseColor;
                float4 _BaseMap_ST;
                float _VertexSnap;
                float _AffineWarp;
                float _LightIntensity;
                float _ShadowStrength;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS);

                float4 clipPos = posInputs.positionCS;

                float2 ndc = clipPos.xy / clipPos.w;
                float snap = max(_VertexSnap, 1.0);
                ndc = round(ndc * snap) / snap;
                clipPos.xy = ndc * clipPos.w;

                OUT.positionCS = clipPos;
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.normalWS = normalize(normalInputs.normalWS);
                OUT.positionWS = posInputs.positionWS;
                OUT.fogCoord = ComputeFogFactor(clipPos.z);
                OUT.screenPos = ComputeScreenPos(clipPos);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float2 uv = IN.uv;

                if (_AffineWarp > 0.0001)
                {
                    float2 screenUV = IN.screenPos.xy / IN.screenPos.w;
                    uv += (screenUV - 0.5) * _AffineWarp * 0.05;
                }

                half4 texColor = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, uv) * _BaseColor;

                Light mainLight = GetMainLight();
                float3 normalWS = normalize(IN.normalWS);
                float NdotL = saturate(dot(normalWS, mainLight.direction));

                float lightTerm = lerp(1.0 - _ShadowStrength, 1.0, NdotL) * _LightIntensity;
                half3 litColor = texColor.rgb * mainLight.color * lightTerm;

                half3 ambient = texColor.rgb * 0.2;
                half3 finalColor = litColor + ambient;

                finalColor = MixFog(finalColor, IN.fogCoord);

                return half4(finalColor, texColor.a);
            }
            ENDHLSL
        }
    }
}
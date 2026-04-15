Shader "Hidden/Custom/PSXPost"
{
    Properties
    {
        _MainTex("Source", 2D) = "white" {}
        _ColorSteps("Color Steps", Float) = 32
        _DitherStrength("Dither Strength", Range(0,1)) = 0.25
    }

    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Name "PSXPostPass"
            ZTest Always Cull Off ZWrite Off

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            TEXTURE2D(_BlitTexture);
            SAMPLER(sampler_BlitTexture);

            CBUFFER_START(UnityPerMaterial)
                float _ColorSteps;
                float _DitherStrength;
            CBUFFER_END

            Varyings Vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                return OUT;
            }

            float Bayer4x4(int2 pixelPos)
            {
                int x = pixelPos.x % 4;
                int y = pixelPos.y % 4;

                const float bayer[16] =
                {
                    0.0/16.0,  8.0/16.0,  2.0/16.0, 10.0/16.0,
                    12.0/16.0, 4.0/16.0, 14.0/16.0, 6.0/16.0,
                    3.0/16.0, 11.0/16.0, 1.0/16.0,  9.0/16.0,
                    15.0/16.0, 7.0/16.0, 13.0/16.0, 5.0/16.0
                };

                return bayer[y * 4 + x];
            }

            half4 Frag(Varyings IN) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_BlitTexture, IN.uv);

                int2 pixelPos = int2(IN.positionCS.xy);
                float dither = (Bayer4x4(pixelPos) - 0.5) * _DitherStrength;

                float steps = max(_ColorSteps, 2.0);
                col.rgb += dither / steps;
                col.rgb = floor(col.rgb * steps) / steps;

                return col;
            }
            ENDHLSL
        }
    }
}
Shader "Custom/CharacterBlink"
{
    Properties
    {
        _MainTex     ("Main Texture", 2D)        = "white" {}
        _BaseColor   ("Base Color", Color)        = (1, 1, 1, 1)
        _BlinkColor  ("Blink Color", Color)       = (1, 0, 0, 1)
        _BlinkSpeed  ("Blink Speed", Float)       = 5.0
        _BlinkAmount ("Blink Amount", Range(0,1)) = 0.0
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline" = "UniversalPipeline"
            "RenderType"     = "Opaque"
            "Queue"          = "Geometry"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex   vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);

            CBUFFER_START(UnityPerMaterial)
                float4 _MainTex_ST;
                float4 _BaseColor;
                float4 _BlinkColor;
                float  _BlinkSpeed;
                float  _BlinkAmount;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv          = TRANSFORM_TEX(IN.uv, _MainTex);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 texCol = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);
                texCol *= _BaseColor;

                float blink = (sin(_Time.y * _BlinkSpeed) + 1.0) * 0.5;

                half4 final  = lerp(texCol, _BlinkColor, blink * _BlinkAmount);
                final.a      = texCol.a;
                return final;
            }
            ENDHLSL
        }
    }

    FallBack "Universal Render Pipeline/Lit"
}
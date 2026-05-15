Shader "Custom/CurvedWorld"
{
    Properties
    {
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Color ("Color", Color) = (1,1,1,1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard vertex:vert addshadow
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Global curve values — set from C# script
        float3 _CurveStrength;   // x = sideways, y = vertical, z = unused
        float3 _CurveOrigin;     // usually camera position

        void vert(inout appdata_full v)
        {
            // Convert vertex to world space
            float4 worldPos = mul(unity_ObjectToWorld, v.vertex);

            // Distance from curve origin (camera) along Z
            float dist = worldPos.z - _CurveOrigin.z;
            float distSqr = dist * dist;

            // Apply curve - bends down and to the side based on distance
            worldPos.y += distSqr * _CurveStrength.y * -0.001;
            worldPos.x += distSqr * _CurveStrength.x *  0.001;

            // Convert back to object space
            v.vertex = mul(unity_WorldToObject, worldPos);
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
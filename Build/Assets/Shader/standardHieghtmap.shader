Shader "Custom/standardHieghtmap"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _HeightmapTex;

        struct Input
        {
            float2 uv_MainTex;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void vert(inout appdata_full v)
        {
            /*float2 reverse_uv = float2(1 - v.texcoord.x, 1 - v.texcoord.y);
            v.vertex.y = tex2Dlod(_HeightmapTex, float4(reverse_uv.xy, 0, 0)).r * 900;*/
            v.vertex.y = tex2Dlod(_HeightmapTex, float4(v.texcoord.xy, 0, 0)).r * 900; // for multiple
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            /*float2 reverse_uv = float2(1 - IN.uv_MainTex.x, 1 - IN.uv_MainTex.y);
            fixed4 c = tex2D(_MainTex, reverse_uv) * _Color;
            fixed4 constraint = tex2D(_HeightmapTex, reverse_uv).g * fixed4(1, 0, 0, 1) + (1 - tex2D(_HeightmapTex, reverse_uv).g) * c;*/
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            fixed4 constraint = tex2D(_HeightmapTex, IN.uv_MainTex).g * fixed4(1, 0, 0, 1) + (1 - tex2D(_HeightmapTex, IN.uv_MainTex).g) * c;
            o.Albedo = constraint.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}

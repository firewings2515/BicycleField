// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Terrain/NNI"
{
    Properties
    {
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("Albedo (RGB)", 2D) = "white" {}
        _Glossiness("Smoothness", Range(0,1)) = 0.5
        _Metallic("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        Cull Off
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;
        int features_count;
        float height_base;
        float4 features[512];

        struct Input {
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

        float NNI(float x, float z)
        {
            float d_min = sqrt(pow(features[0].x - x, 2) + pow(features[0].z - z, 2));
            int p_index = 0;
            for (int i = 1; i < features_count; i++)
            {
                if (abs(features[i].x - x) < 320.0 && abs(features[i].z - z) < 320.0)
                {
                    float dist = sqrt(pow(features[i].x - x, 2) + pow(features[i].z - z, 2));
                    if (d_min > dist)
                    {
                        d_min = dist;
                        p_index = i;
                    }
                }
            }
            return features[p_index].y;
        }

        void vert(inout appdata_full v)
        {
            float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
            float4 nni_vertex = float4(worldPos.x, NNI(worldPos.x, worldPos.z) + height_base, worldPos.z, worldPos.w);
            float dxz = 1;
            float3 dx = float3(worldPos.x + dxz, NNI(worldPos.x + dxz, worldPos.z) + height_base, worldPos.z) - float3(nni_vertex.x, nni_vertex.y, nni_vertex.z);
            float3 dz = float3(worldPos.x, NNI(worldPos.x, worldPos.z + dxz) + height_base, worldPos.z + dxz) - float3(nni_vertex.x, nni_vertex.y, nni_vertex.z);
            v.normal = normalize(cross(dx, dz));
            v.vertex = mul(unity_WorldToObject, nni_vertex);
        }

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            /*if (features_count > 60)
                c = fixed4(0, (features_count - 50) / 20.0 * 255, 0, 1);
            else
                c = fixed4((features_count - 50) / 20.0 * 255, 0, 0, 1);*/
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
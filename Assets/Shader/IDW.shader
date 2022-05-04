// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

// Upgrade NOTE: replaced '_World2Object' with 'unity_WorldToObject'

Shader "Terrain/IDW"
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
        float dxz;
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

        float getWeight(float d)
        {
            float f = pow(d, 1);
            if (f < 0.000001)
                return 0.000001;
            return 1 / f;
        }

        float IDW(float x, float z)
        {
            float sum_up = 0.0;
            float sum_down = 0.0;
            for (int i = 0; i < features_count; i++)
            {
                float dist = sqrt(pow(features[i].x - x, 2) + pow(features[i].z - z, 2));
                if (dist < 320.0)
                {
                    sum_up += getWeight(dist) * features[i].y;
                    sum_down += getWeight(dist);
                }
            }
            if (sum_down < 0.000001)
                sum_down = 0.000001;
            return sum_up / sum_down;
        }

        void vert(inout appdata_full v)
        {
            float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
            float4 idw_vertex = float4(worldPos.x, IDW(worldPos.x, worldPos.z) + height_base, worldPos.z, worldPos.w);
            float3 dpx = float3(worldPos.x + dxz, IDW(worldPos.x + dxz, worldPos.z) + height_base, worldPos.z) - float3(idw_vertex.x, idw_vertex.y, idw_vertex.z);
            float3 dpz = float3(worldPos.x, IDW(worldPos.x, worldPos.z + dxz) + height_base, worldPos.z + dxz) - float3(idw_vertex.x, idw_vertex.y, idw_vertex.z);
            float3 dnx = float3(worldPos.x - dxz, IDW(worldPos.x - dxz, worldPos.z) + height_base, worldPos.z) - float3(idw_vertex.x, idw_vertex.y, idw_vertex.z);
            float3 dnz = float3(worldPos.x, IDW(worldPos.x, worldPos.z - dxz) + height_base, worldPos.z - dxz) - float3(idw_vertex.x, idw_vertex.y, idw_vertex.z);
            v.vertex = mul(unity_WorldToObject, idw_vertex);
            float3 n1 = normalize(cross(dpx, dpz));
            float3 n2 = normalize(cross(dpz, dnx));
            float3 n3 = normalize(cross(dnx, dnz));
            float3 n4 = normalize(cross(dnz, dpx));
            float3 n = normalize((n1 + n2 + n3 + n4) / 4);
            TANGENT_SPACE_ROTATION;
            float3 tangentSpaceNormal = mul(rotation, n);
            v.normal = normalize(tangentSpaceNormal);
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
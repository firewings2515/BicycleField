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
            LOD 200

            CGPROGRAM
            // Physically based Standard lighting model, and enable shadows on all light types
            #pragma surface surf Standard fullforwardshadows vertex:vert

            // Use shader model 3.0 target, to get nicer looking lighting
            #pragma target 3.0

            sampler2D _MainTex;
            int features_count;
            float height_base;
            float4 features[110];

            /*struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };*/

            //struct v2f
            //{
            //    float3 position : SV_POSITION;
            //    float2 uv_MainTex : TEXCOORD0;
            //};

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
                    float f = d * d;
                    if (f < 0.000001)
                        return 1.0;
                    return 1 / f;
                }

                float IDW(float x, float z)
                {
                    float sum_up = 0.0;
                    float sum_down = 0.0;
                    for (int i = 0; i < features_count; i++)
                    {
                        if (abs(features[i].x - x) < 320.0 && abs(features[i].z - z) < 320.0)
                        {
                            float dist = sqrt(pow(features[i].x - x, 2) + pow(features[i].z - z, 2));
                            sum_up += getWeight(dist) * features[i].y;
                            sum_down += getWeight(dist);
                        }
                    }
                    if (sum_down < 0.000001)
                        sum_down = 1.0;
                    return sum_up / sum_down;
                }

                void vert(inout appdata_full v)
                {
                    //v.vertex.y += features[((int)(v.texcoord.x * 4) + (int)(v.texcoord.y * 4) * 5)].y;
                    //v.vertex.y = IDW2(v.vertex.x, v.vertex.z);
                    float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                    float4 idw_vertex = float4(worldPos.x, IDW(worldPos.x, worldPos.z) + height_base, worldPos.z, worldPos.w);
                    v.vertex = mul(unity_WorldToObject, idw_vertex);
                    /*float3 p = (unity_ObjectToWorld * gl_Vertex).xyz;
                    p.y = IDW2(p.x, p.z);
                    v.vertex = gl_ModelViewProjectionMatrix * p;*/
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

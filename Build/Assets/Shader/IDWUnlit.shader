// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Unlit/IDWUnlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            float4 area_features[128];
            int area_features_count;

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
                for (int i = 0; i < area_features_count; i++)
                {
                    if (abs(area_features[i].x - x) < 320.0 && abs(area_features[i].z - z) < 320.0)
                    {
                        float dist = sqrt(pow(area_features[i].x - x, 2) + pow(area_features[i].z - z, 2));
                        sum_up += getWeight(dist) * area_features[i].y;
                        sum_down += getWeight(dist);
                    }
                }
                if (sum_down < 0.000001)
                    sum_down = 1.0;
                return sum_up / sum_down;
            }

            v2f vert (appdata v)
            {
                v2f o;
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                float4 idw_vertex = float4(worldPos.x, IDW(worldPos.x, worldPos.z), worldPos.z, worldPos.w);
                o.vertex = mul(UNITY_MATRIX_VP, idw_vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}

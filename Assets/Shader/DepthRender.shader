Shader "Hidden/DepthRender"
{
    Properties
    {
        _MainTex("Texture", 2D) = "black" {}
    }
        SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            sampler2D _CameraDepthTexture;
            sampler2D _MainTex;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float4 col = float4(1, 1, 1, 1);

                // ?¸m ²`«×?¨ú
                float depth = tex2D(_CameraDepthTexture, i.uv).r;
                col = float4(depth, depth, depth, 1);
                return col;
            }
            ENDCG
        }
    }
}
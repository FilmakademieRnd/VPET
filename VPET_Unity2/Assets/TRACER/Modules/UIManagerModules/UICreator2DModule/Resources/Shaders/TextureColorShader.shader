Shader "Custom/TextureColorShader"
{
    Properties
    {
        _InputColor("InputColor", Color) = (1, 1, 1, 1)
    }
        SubShader
    {
        Pass
        {
            //CGPROGRAM
            //#pragma vertex vert
            //#pragma fragment frag
            //// make fog work
            ////#pragma multi_compile_fog

            //#include "UnityCG.cginc"

            //struct appdata
            //{
            //    float4 vertex : POSITION;
            //    float2 uv : TEXCOORD0;
            //};

            //struct v2f
            //{
            //    float2 uv : TEXCOORD0;
            //    UNITY_FOG_COORDS(1)
            //    float4 vertex : SV_POSITION;
            //};

            //sampler2D _MainTex;
            //float4 _MainTex_ST;

            //v2f vert (appdata v)
            //{
            //    v2f o;
            //    o.vertex = UnityObjectToClipPos(v.vertex);
            //    o.uv = TRANSFORM_TEX(v.uv, _MainTex);
            //    UNITY_TRANSFER_FOG(o,o.vertex);
            //    return o;
            //}

            //fixed4 frag (v2f i) : SV_Target
            //{
            //    // sample the texture
            //    fixed4 col = tex2D(_MainTex, i.uv);
            //    //// apply fog
            //    //UNITY_APPLY_FOG(i.fogCoord, col);
            //    //return col;
            //    return fixed4(1.0, 0.0, 0.0, 1.0)
            //}
            //ENDCG
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float4 _InputColor;

            struct vertexInput {
                float4 vertex : POSITION;
                float4 texcoord0 : TEXCOORD0;
            };

            struct fragmentInput {
                float4 position : SV_POSITION;
                float4 texcoord0 : TEXCOORD0;
            };

            
            float3 gradient(in float c) {
                float3 rgb = clamp(abs(fmod(c * 6.0 + float3(0.0,4.0,2.0),6.0) - 3.0) - 1.0,0.0,1.0);
                return rgb = rgb * rgb * (3.0 - 2.0 * rgb);
            }

            //float4 vert(appdata_base v) : POSITION {
            //    return UnityObjectToClipPos(v.vertex);
            //}

            fragmentInput vert(vertexInput i) {
                fragmentInput o;
                o.position = UnityObjectToClipPos(i.vertex);
                o.texcoord0 = i.texcoord0;
                return o;
            }


            fixed4 frag(fragmentInput i) : SV_Target{
                float3 inColor = _InputColor;
                float2 st = i.texcoord0.xy;
                float3 color = float3(0,0,0);

                if (st.x < 0.5)
                    color = gradient(st.y);
                else
                    color = lerp(float3(1, 1, 1), inColor, st.y);

                return fixed4(color, 1.0);
            }

            ENDCG
        }
    }
}

// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/BasicColorShader"
{
    Properties
    {
        //_MainTex ("Texture", 2D) = "white" {}
        _Input("Input", Color) = (1, 1, 1, 1)
    }
        SubShader
    {
        //Tags { "RenderType"="Opaque" }
        //LOD 100

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

            float4 _Input;

            float3 gradient(in float c) {
                float3 rgb = clamp(abs(fmod(c * 6.0 + float3(0.0,4.0,2.0),6.0) - 3.0) - 1.0,0.0,1.0);
                return rgb = rgb * rgb * (3.0 - 2.0 * rgb);
            }

            float4 vert(appdata_base v) : POSITION {
                return UnityObjectToClipPos(v.vertex);
            }



            fixed4 frag(float4 sp:VPOS, float3 localPosition:TEXCOORD0) : SV_Target {
                float3 inColor = float3(0.076,0.142,0.995);
                
                float2 st = sp.xy / _ScreenParams.xy;
                float3 color = float3(0,0,0);

                if (st.x < 0.5)
                    color = gradient(st.y);
                else
                    color = lerp(float3(1, 1, 1), _Input, st.y);

                return fixed4(color, 1.0);
            }

            ENDCG
        }
    }
}

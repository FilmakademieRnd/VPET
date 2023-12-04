
Shader "VPET/GizmoShader" {
    Properties{
    }

        Category{
            Tags { "IgnoreProjector" = "True" }
           
            Cull Off 
            Lighting Off 
            ZWrite Off
            ZTest Always

            SubShader {
                Pass {

                    CGPROGRAM
                    #pragma vertex vert
                    #pragma fragment frag
                    #pragma target 2.0

                    #include "UnityCG.cginc"

                    struct appdata_t {
                        float4 vertex : POSITION;
                        fixed4 color : COLOR;
                    };

                    struct v2f {
                        float4 vertex : SV_POSITION;
                        fixed4 color : COLOR;
                        UNITY_VERTEX_OUTPUT_STEREO
                    };

                    v2f vert(appdata_t v)
                    {
                        v2f o;
                        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                        o.vertex = UnityObjectToClipPos(v.vertex);
                        o.color = v.color;
                        return o;
                    }

                    fixed4 frag(v2f i) : SV_Target
                    {
                        return i.color;
                    }
                    ENDCG
                }
            }
        }
}
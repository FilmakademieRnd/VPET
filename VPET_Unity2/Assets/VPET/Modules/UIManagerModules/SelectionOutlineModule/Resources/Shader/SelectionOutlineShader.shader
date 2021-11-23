Shader "Outline"
{
    Properties
    {
        _Color("Main Color", Color) = (0,0,0,1)

        _OutlineColor("Outline color", Color) = (1,1,1,1)
        _OutlineWidth("Outline width", Range(0.0, 2.0)) = 0.15
    }
        CGINCLUDE
#include "UnityCG.cginc"

        struct appdata {
        float4 vertex : POSITION;
        float4 normal : NORMAL;
    };

    static const float _Angle = 89;

    uniform float4 _OutlineColor;
    uniform float _OutlineWidth;

    uniform float4 _Color;

    ENDCG

        // stencil
        SubShader{
            Pass {
                Tags{
                "Queue" = "Transparent-1"
                "IgnoreProjector" = "True"
                }
                Cull Off
                ZWrite On
                ZTest Always
                Stencil {
                    Ref 1
                    Comp Always
                    Pass Replace
                }
                ColorMask 0
            }

        // outline
        Pass{
            Tags{   "Queue" = "Transparent"
                    "IgnoreProjector" = "True"
                    "RenderType" = "Transparent"
            }
            Stencil {
                Ref 1
                Comp NotEqual
            }

            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite on
            ZTest Always
            Cull Off
            CGPROGRAM

            struct v2f {
                float4 pos : SV_POSITION;
            };

            #pragma vertex vert
            #pragma fragment frag

            v2f vert(appdata v) {

                float vPosZ = UnityObjectToViewPos(v.vertex).z * 0.01;
                float3 scaleDir = normalize(v.vertex.xyz - float4(0,0,0,1));

                if (degrees(acos(dot(scaleDir.xyz, v.normal.xyz))) > _Angle) {
                    v.vertex.xyz += normalize(v.normal.xyz) * _OutlineWidth * -vPosZ;
                }
                else {
                   v.vertex.xyz += scaleDir * _OutlineWidth * -vPosZ;
                }

                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            half4 frag(v2f i) : COLOR{
                return _OutlineColor;
            }
        ENDCG
        }
    }
        Fallback "Diffuse"
}
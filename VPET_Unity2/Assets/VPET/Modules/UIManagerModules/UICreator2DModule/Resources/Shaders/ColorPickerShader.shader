Shader "Custom/ColorPickerShader"
{
    Properties
    {
        _InputColor("InputColor", Color) = (1, 1, 1, 1)
        _InputPos("ColorPosition", Vector) = (0, 0, 0, 0)
    }
        SubShader
    {
        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float4 _InputColor;
            float4 _InputPos;

            struct vertexInput 
            {
                float4 vertex : POSITION;
                float4 texcoord0 : TEXCOORD0;
            };

            struct fragmentInput 
            {
                float4 position : SV_POSITION;
                float4 texcoord0 : TEXCOORD0;
            };

            // Linear Hue gradient
            float3 gradient(in float c) 
            {
                float3 rgb = clamp(abs(fmod(c * 6.0 + float3(0.0, 4.0, 2.0), 6.0) - 3.0) - 1.0, 0.0, 1.0);
                return rgb = rgb * rgb * (3.0 - 2.0 * rgb);
            }

            fragmentInput vert(vertexInput i)
            {
                fragmentInput o;
                o.position = UnityObjectToClipPos(i.vertex);
                o.texcoord0 = i.texcoord0;
                return o;
            }

            fixed4 frag(fragmentInput i) : SV_Target
            {
                // Indicator size parameters
                float dotWid = .024;
                float2 cirOff = { 0, .135 };
                float cirWid = .135;
                float recWid = .1;
                float recHei = .06;
                float smooth = .01;

                float3 inColor = _InputColor;
                float2 st = i.texcoord0.xy;
                float3 color = 0;

                float4 inPos = _InputPos;

                // Color picker design
                if (st.x > 0.8)
                    color = gradient(st.y);
                else
                {
                    color = lerp(1, inColor, st.x * 1.25);
                    color = lerp(0, color, st.y);
                }

                // UI Indicators
                // Tiny dot
                float3 dot = 0;
                float dist = distance(inPos.xy, st);
                //dot += smoothstep(dotWid - .8*smooth, dotWid, dist);
                //dot -= smoothstep(dotWid, dotWid + .8*smooth, dist);
                //dot -= smoothstep(dotWid + smooth, dotWid + 2*smooth, dist);
                //dot = clamp(dot, 0, 1);
                //dot += smoothstep(dotWid + 2 * smooth, dotWid + 3 * smooth, dist);
                //dot -= smoothstep(dist, dotWid, dotWid + smooth);
                //dot = clamp(dot, 0, 1);
                //dot = 1 - dot;
                float dotIn = smoothstep(dotWid - .8 * smooth, dotWid, dist);
                float dotOu = smoothstep(dotWid, dotWid + .8 * smooth, dist);
                // adjust
                float fade = distance(0, float2(1.25*st.x, 1-st.y));
                fade = smoothstep(.4, .6, fade);
                float flip = 2 * fade - 1;
                dot = flip * dotIn - flip * dotOu;

                // Ring - unused
                float3 ring = 0;
                ring -= smoothstep(dotWid, dotWid + smooth, dist);
                ring += smoothstep(dotWid - .0 * smooth, dotWid + 2 * smooth, dist);
                //ring = clamp(ring, 0, 1);

                // Double circle
                float3 circles = 0;
                dist = distance(inPos.zw + cirOff, st);
                circles += smoothstep(cirWid, cirWid + smooth, dist);
                dist = distance(inPos.zw - cirOff, st);
                circles -= 1 - smoothstep(cirWid, cirWid + smooth, dist);
                circles = clamp(circles, 0, 1);

                // Rectangle
                float3 rect = 0;
                float2 cor = step(float2(-recWid, -recHei), st - inPos.zw);
                float rec = cor.x * cor.y;
                cor = step(float2(-recWid, -recHei), -st + inPos.zw);
                rec *= cor.x * cor.y;
                rect += rec;

                // Combine all
                color += rect * circles + dot;

                return fixed4(color, 1.0);
            }

            ENDCG
        }
    }
}

Shader "VPET/ARCameraShaderChromaKey"
{
	Properties
	{
    	_textureY ("TextureY", 2D) = "white" {}
        _textureCbCr ("TextureCbCr", 2D) = "black" {}
		[Header(Keying Settings)]
		_KeyColor("Key", Color) = (1,1,1)
		_Radius("Radius", Range(0, 1)) = 0.0
		_Threshold("Threshold", Range(0, 1)) = 0.1
		_Softrange("SoftRange", Range(0, 1)) = 0.1
		_Depth("Depth", Float) = 100.0
	}
	SubShader
	{
		Cull Off
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }		
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
            ZWrite Off
			ZTest Off
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#include "UnityCG.cginc"
			#include "VPETCG.cginc"

            CBUFFER_START(UnityPerFrame)
            // Device display transform is provided by the AR Foundation camera background renderer.
            float4x4 _UnityDisplayTransform;
            CBUFFER_END

			// keying properties
			fixed3 _KeyColor;
			float _Radius;
			float _Threshold;
			float _Softrange;
			float _Depth;

            // samplers
            sampler2D _textureY;
            sampler2D _textureCbCr;

			struct Vertex
			{
				float4 position : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct TexCoordInOut
			{
				float4 position : SV_POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct fragOutput 
			{
				fixed4 color : SV_Target;
				float depth: SV_Depth;
			};

			TexCoordInOut vert (Vertex vertex)
			{
				TexCoordInOut o;
				o.position = UnityObjectToClipPos(vertex.position); 

				float texX = vertex.texcoord.x;
				float texY = vertex.texcoord.y;
				
				o.texcoord.x = (_UnityDisplayTransform[0].x * texX + _UnityDisplayTransform[1].x * (texY) + _UnityDisplayTransform[2].x);
 			 	o.texcoord.y = (_UnityDisplayTransform[0].y * texX + _UnityDisplayTransform[1].y * (texY) + (_UnityDisplayTransform[2].y));
	            
				return o;
			}
			
			fragOutput frag (TexCoordInOut i)
			{
				fragOutput o;
				// sample the texture
                float2 texcoord = i.texcoord;
                float y = tex2D(_textureY, texcoord).r;
                float4 ycbcr = float4(y, tex2D(_textureCbCr, texcoord).rg, 1.0);

                //texcoord = floor(texcoord.x/0.015) + floor(texcoord.y/0.015);
                //texcoord = frac(texcoord * 0.5);
                //texcoord *= 2;

				const float4x4 ycbcrToRGBTransform = float4x4(
						float4(1.0, +0.0000, +1.4020, -0.7010),
						float4(1.0, -0.3441, -0.7141, +0.5291),
						float4(1.0, +1.7720, +0.0000, -0.8860),
						float4(0.0, +0.0000, +0.0000, +1.0000)
					);

                float4 rgba = mul(ycbcrToRGBTransform, ycbcr);


				fixed3 colHCV = RGBtoHCV(rgba.rgb);
				fixed3 keyHCV = RGBtoHCV(_KeyColor);

				if (abs(colHCV.x - keyHCV.x) < _Radius && colHCV.y > _Threshold)
				{
					float delta = colHCV.y - _Threshold;
					if (delta < _Softrange)
					{
						rgba.a = 1-delta;
						// rgba.rgb = saturate(rgba.rgb + _Tint*(1-delta));
					}
					else
					{
						rgba.a = 0;
						rgba.rgb = saturate(rgba.rgb);
					}
				}

				o.color = rgba;
                //o.color = float4(texcoord,0.0,1.0);
				//o.depth = _Depth;

				return o;

			}
			ENDCG
		}
	}
}

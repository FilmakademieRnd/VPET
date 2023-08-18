Shader "VPET/ARCameraShaderChromaKeyProj"
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
        _cropScaleX("CropScaleX", Range(0, 2)) = 0.25
        _cropScaleY("CropScaleY", Range(0, 2)) = 1.0
	}
	SubShader
	{
		Cull Off
		Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }		
		LOD 100
		Blend SrcAlpha OneMinusSrcAlpha

		Pass
		{
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

            // samplers
            sampler2D _textureY;
            sampler2D _textureCbCr;

            float _cropScaleX;
            float _cropScaleY;

			struct Vertex
			{
				float4 position : POSITION;
				float2 texcoord : TEXCOORD0;
			};

			struct TexCoordInOut
			{
				float4 position : SV_POSITION;
				float4 texcoord : TEXCOORD0;
			};

			struct fragOutput 
			{
				fixed4 color : SV_Target;
			};

			TexCoordInOut vert (Vertex vertex)
			{
				TexCoordInOut o;
				o.position = UnityObjectToClipPos(vertex.position); 

				float4 screenPos = ComputeScreenPos(o.position);

				o.texcoord.x = screenPos.x;
 			 	o.texcoord.y = screenPos.y;
               	o.texcoord.w = screenPos.w;
				return o;
			}
			
			fragOutput frag (TexCoordInOut i)
			{
				fragOutput o;
				// sample the texture
                float2 texcoord = i.texcoord.xy  / i.texcoord.w;
                texcoord.x =        (_UnityDisplayTransform[0].x * texcoord.x + _UnityDisplayTransform[1].x * (texcoord.y) + _UnityDisplayTransform[2].x);
                texcoord.y = 1.0 -  (_UnityDisplayTransform[0].y * texcoord.x + _UnityDisplayTransform[1].y * (texcoord.y) + _UnityDisplayTransform[2].y);
                texcoord.x = texcoord.x + (0.5 - texcoord.x) * _cropScaleX;
                texcoord.y = texcoord.y + (0.5 - texcoord.y) * _cropScaleY;

                //texcoord = floor(texcoord.x/0.015) + floor(texcoord.y/0.015);
                //texcoord = frac(texcoord * 0.5);
                //texcoord *= 2;

                float y = tex2D(_textureY, texcoord).r;
                float4 ycbcr = float4(y, tex2D(_textureCbCr, texcoord).rg, 1.0);

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

                //o.color = float4(texcoord.xy,0.0,1.0);
				o.color = rgba;

				return o;

			}
			ENDCG
		}
	}
}

Shader "VPET/ARCameraShaderChromaKeyProjStd"
{
	Properties {
		_Color ("Color", Color) = (1,1,1,1)
		_MainTex ("Albedo (RGB)", 2D) = "white" {}
		_Glossiness ("Smoothness", Range(0,1)) = 0.5
		_Metallic ("Metallic", Range(0,1)) = 0.0
		[Header(AR Textures)]
    	_textureY ("TextureY", 2D) = "white" {}
        _textureCbCr ("TextureCbCr", 2D) = "black" {}		
		_BlendAR ("BlendAR", Range(0,1)) = 1.0
		[Header(Keying Settings)]
		_KeyColor("Key", Color) = (1,1,1)
		_Radius("Radius", Range(0, 1)) = 0.0
		_Threshold("Threshold", Range(0, 1)) = 0.1
		_Softrange("SoftRange", Range(0, 1)) = 0.1		
	}
	SubShader {
		Tags { "RenderType"="Opaque" }
		LOD 200

		CGPROGRAM
		// Physically based Standard lighting model, and enable shadows on all light types
		#pragma surface surf Standard fullforwardshadows

		// Use shader model 3.0 target, to get nicer looking lighting
		#pragma target 3.0
		
		#include "VPETCG.cginc"

		sampler2D _MainTex;
			
        CBUFFER_START(UnityPerFrame)
        // Device display transform is provided by the AR Foundation camera background renderer.
        float4x4 _UnityDisplayTransform;
        CBUFFER_END

		// surface material props
		half _Glossiness;
		half _Metallic;
		fixed4 _Color;

		// keying properties
		fixed3 _KeyColor;
		float _Radius;
		float _Threshold;
		float _Softrange;

		// samplers
		sampler2D _textureY;
		sampler2D _textureCbCr;
		float _BlendAR;

		struct Input 
		{
			float2 uv_MainTex;
			float4 screenPos;
		};


		// Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
		// See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
		// #pragma instancing_options assumeuniformscaling
		UNITY_INSTANCING_BUFFER_START(Props)
		// put more per-instance properties here
		UNITY_INSTANCING_BUFFER_END(Props)

		void surf (Input IN, inout SurfaceOutputStandard o) 
		{

			float texX = IN.screenPos.x / IN.screenPos.w;
			float texY = IN.screenPos.y / IN.screenPos.w;

			float2 texcoord;
			texcoord.x = (_UnityDisplayTransform[0].x * texX + _UnityDisplayTransform[1].x * (texY) + _UnityDisplayTransform[2].x);
 			texcoord.y = 1-(_UnityDisplayTransform[0].y * texX + _UnityDisplayTransform[1].y * (texY) + (_UnityDisplayTransform[2].y));

			// sample the texture
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

			// blend ar texture
			rgba.a = _BlendAR;

			// Albedo comes from a texture tinted by color
			fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			// combine tinted main texture color with our keyed ycbcr texture
			o.Albedo = c.rgb * (1.0-rgba.a) + rgba.rgb * rgba.a;
			// Metallic and smoothness come from slider variables
			o.Metallic = _Metallic;
			o.Smoothness = _Glossiness;
			o.Alpha = c.a;
		}
		ENDCG
	}
	FallBack "Diffuse"
}

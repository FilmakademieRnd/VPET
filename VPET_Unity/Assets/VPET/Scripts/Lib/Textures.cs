using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vpet
{

	public static class Textures
	{

        //!
        //! function that determines if a texture has alpha
        //! @param  texture   the texture to be checked
        //!
        public static bool hasAlpha(Texture2D texture)
        {
            TextureFormat textureFormat = texture.format;
            return (textureFormat == TextureFormat.Alpha8 ||
                textureFormat == TextureFormat.ARGB4444 ||
                textureFormat == TextureFormat.ARGB32 ||
                textureFormat == TextureFormat.DXT5 ||
                textureFormat == TextureFormat.PVRTC_RGBA2 ||
                textureFormat == TextureFormat.PVRTC_RGBA4 ||
                textureFormat == TextureFormat.ATC_RGBA8);
        }
	

	}
}
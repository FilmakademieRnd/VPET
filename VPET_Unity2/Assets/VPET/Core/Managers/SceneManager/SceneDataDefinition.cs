/*
VPET - Virtual Production Editing Tools
vpet.research.animationsinstitut.de
https://github.com/FilmakademieRnd/VPET

Copyright (c) 2023 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

This project has been initiated in the scope of the EU funded project
Dreamspace (http://dreamspaceproject.eu/) under grant agreement no 610005 2014-2016.

Post Dreamspace the project has been further developed on behalf of the
research and development activities of Animationsinstitut.

In 2018 some features (Character Animation Interface and USD support) were
addressed in the scope of the EU funded project SAUCE (https://www.sauceproject.eu/)
under grant agreement no 780470, 2018-2022

This program is free software; you can redistribute it and/or modify it under
the terms of the MIT License as published by the Open Source Initiative.

This program is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE. See the MIT License for more details.

You should have received a copy of the MIT License along with
this program; if not go to
https://opensource.org/licenses/MIT
*/

//! @file "SceneDataDefinition.cs"
//! @brief definition of VPET scene data structure
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 11.03.2022

using UnityEngine;
using System.Runtime.InteropServices;

namespace vpet
{
    public partial class SceneManager : Manager
    {
        public static string[] shaderKeywords = { "_NORMALMAP", "_ALPHATEST_ON", "_ALPHABLEND_ON", "_ALPHAPREMULTIPLY_ON", "_EMISSION", "_PARALLAXMAP", "_DETAIL_MULX2", "_METALLICGLOSSMAP", "_SPECGLOSSMAP" };

        //!
        //! Enumeration defining VPET node Types.
        //!
        public enum NodeType { GROUP, GEO, LIGHT, CAMERA, SKINNEDMESH }

        //!
        //! Data structure for serialising basic SceneNodes.
        //! The struct layout and array size for mashaling  has to be fixed
        //! to be compatible with unmanaged code.
        //!
        [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Auto)]
        public class SceneNode
        {
            //! Flag that determines whether a node is editable or not. 
            public bool editable;
            //! The Nuber of childes the node have. 
            public int childCount;
            //! The position of the node in world space, stored as float array with the legth of 3.
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public float[] position;
            //! The scale of the node in world space, stored as float array with the legth of 3.
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public float[] scale;
            //! The rotation of the node in world space, stored as float array with the legth of 4.
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] rotation;
            //! The name of the node, stored as byte array with the legth of 64.
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 64)]
            public byte[] name;
        };

        //!
        //! Data structure for serialising SceneNodes representing Unity
        //! game objects containing 3d scene components like meshes and materials.
        //!
        [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Auto)]
        public class SceneNodeGeo : SceneNode
        {
            //! The ID for referencing the associated geometry data.
            public int geoId;
            //! The ID for referencing the associated material data.
            public int materialId;
            //! The color if the node has no material assigned, stored as float array with the legth of 4.
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] color;
        };

        //!
        //! Data structure for serialising SceneNodes representing Unity
        //! game objects containing skinned mesh components.
        //!
        [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Auto)]
        public class SceneNodeSkinnedGeo : SceneNodeGeo
        {
            //! The length of the array storing the bind poses.
            public int bindPoseLength;
            //! The ID for referencing the associated root bone.
            public int rootBoneID;
            //! The bounds if the skinned mesh in world space, stored as float array with ihe legth of 3.
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public float[] boundExtents;
            //! The center if the skinned mesh in world space, stored as float array with ihe legth of 3.
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public float[] boundCenter;
            //! The bind poses of the skinned mesh stored as 99 4x4 matrices in a float array.
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16 * 99)]
            public float[] bindPoses;
            //! The bone IDs for referencing the associated skeleton bones.
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 99)]
            public int[] skinnedMeshBoneIDs;
        };

        //!
        //! Data structure for serialising SceneNodes representing Unity
        //! game objects containing light components.
        //!
        [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Auto)]
        public class SceneNodeLight : SceneNode
        {
            //! Enumeration storing the Unity light type.
            public LightType lightType;
            //! The intensity value of the light.
            public float intensity;
            //! The beam angle of the light. (Only used if LightType is spot.)
            public float angle;
            //! The range value of the light.
            public float range;
            //! The color of the light, stored as float array with the legth of 3.
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
            public float[] color;
        };

        //!
        //! Data structure for serialising SceneNodes representing Unity
        //! game objects containing camera components.
        //!
        [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Auto)]
        public class SceneNodeCam : SceneNode
        {
            //! The field of view of the camera. 
            public float fov;
            //! The aspect ratio of the camera. 
            public float aspect;
            //! The near clipping plane of the camera.
            public float near;
            //! The far clipping plane of the camera.
            public float far;
            //! The focal distance of the camera. 
            public float focalDist;
            //! The aperture of the camera. 
            public float aperture;
        };

        //!
        //! Data structure for serialising the VPET header,
        //! containing scene relevant informations.
        //!
        [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Auto)]
        public class VpetHeader
        {
            //! Global factor for light intensity scaling. 
            public float lightIntensityFactor;
        }

        //!
        //! VPET structure used to hold mesh data.
        //! This sructure is used for saving to disk or sending via network.
        //!
        public class ObjectPackage
        {
            //! The number of vertices in one object package.
            public int vSize;
            //! The number of indices in one object package.
            public int iSize;
            //! The number of normals in one object package.
            public int nSize;
            //! The number of UVs in one object package.
            public int uvSize;
            //! The number of blend weights in one object package.
            public int bWSize;
            //! The additionally stored Unity mesh.
            public Mesh mesh;
            //! The array of floats storing the vertex positions as x,y,z.
            public float[] vertices;
            //! The array of ints storing the indices.
            public int[] indices;
            //! The array of floats storing the normals as x,y,z.
            public float[] normals;
            //! The array of floats storing the UVs as x,y.
            public float[] uvs;
            //! The array of floats storing the bone weights as x,y,z,w per vertex.
            public float[] boneWeights;
            //! The array of ints storing the bone indices.
            public int[] boneIndices;
        };

        //!
        //! VPET structure used to hold additional skinned mesh data.
        //! This sructure is used for saving to disk or sending via network.
        //!
        public class CharacterPackage
        {
            //! The size of the bone mapping array.
            public int bMSize;
            //! The size of the skeleton array.
            public int sSize;
            //! The object ID of the root bone.
            public int rootId;
            //! The array of IDs for referencing the associated bone objects.
            public int[] boneMapping;
            //! The array of IDs for referencing the sceleton objects.
            public int[] skeletonMapping;
            //! The array of bone positions for this character as vec3[].
            public float[] bonePosition;
            //! The array of bone rotations for this character as vec4[].
            public float[] boneRotation;
            //! The array of bone scales for this character as vec3[].
            public float[] boneScale;
        };

        //!
        //! VPET structure used to hold texture data.
        //! This sructure is used for saving to disk or sending via network.
        //!
        public class TexturePackage
        {
            //! The size of the texture in bytes.
            public int colorMapDataSize;
            //! The width of the texture.
            public int width;
            //! The height of the texture.
            public int height;
            //! The Unity texture format.
            public TextureFormat format;
            //! The additionally stored Unity texture.
            public Texture texture;
            //! The array of bytes storing the raw texture data. 
            public byte[] colorMapData;
        };

        //!
        //! VPET structure used to hold material data.
        //! This sructure is used for saving to disk or sending via network.
        //!
        public class MaterialPackage
        {
            //! The type of the material. 0=Unity, 1=External
            public int type;
            //! The name of the material.
            public string name;
            //! The Unity resource name the material uses to refrence its shaders.
            public string src;
            //! The material ID for instancing.
            public int materialID;
            //! The IDs for referencing the associated texture data.
            public int[] textureIds;
            //! The textures UV offset as Vec2.
            public float[] textureOffsets;
            //! The textures UV scale as Vec2.
            public float[] textureScales;
            //! Available shader configuration flags.
            //! [0] "_NORMALMAP"
            //! [1] "_ALPHATEST_ON"
            //! [2] "_ALPHABLEND_ON"
            //! [3] "_ALPHAPREMULTIPLY_ON"
            //! [4] "_EMISSION"
            //! [5] "_PARALLAXMAP"
            //! [6] "_DETAIL_MULX2"
            //! [7] "_METALLICGLOSSMAP"
            //! [8] "_SPECGLOSSMAP"
            public bool[] shaderConfig;
            //! Available shader parameter IDs ( ID, type (not part of this array) )
            //! [0]	"_Color, Color"	
            //! [1]	"_MainTex, Texture"	
            //! [2]	"_Cutoff, float"	
            //! [3]	"_Glossiness, float"
            //! [4]	"_GlossMapScale, float"	
            //! [5]	"_SmoothnessTextureChannel, int"	
            //! [6]	"_Metallic, float"	
            //! [7]	"_MetallicGlossMap, Texture"	
            //! [8]	"_SpecularHighlights, int"
            //! [9]	"_GlossyReflections, int"	
            //! [10] "_BumpScale, int"	
            //! [11] "_BumpMap, Texture"	
            //! [12] "_Parallax, float"	
            //! [13] "_ParallaxMap, Texture"	
            //! [14] "_OcclusionStrength, float"	
            //! [15] "_OcclusionMap, Texture"	
            //! [16] "_EmissionColor, Color"	
            //! [17] "_EmissionMap, Texture"	
            //! [18] "_DetailMask, Texture"	
            //! [19] "_DetailAlbedoMap, Texture"
            //! [20] "_DetailNormalMapScale, int"
            //! [21] "_DetailNormalMap, Texture"
            //! [22] "_UVSec, int"
            //! [23] "_Mode, int"
            //! [24] "_SrcBlend, int"
            //! [25] "_DstBlend, int"
            //! [26] "_ZWrite, int"
            public int[] shaderPropertyIds;
            //! Available shader parameter types
            //! [0]	"Color"	
            //! [1]	"Vector4"	
            //! [2]	"int"	
            //! [3]	"float"
            //! [4]	"Texture"
            public int[] shaderPropertyTypes;
            //! All available shader parameter values as a byte stream.
            public byte[] shaderProperties;
        };
    }
}

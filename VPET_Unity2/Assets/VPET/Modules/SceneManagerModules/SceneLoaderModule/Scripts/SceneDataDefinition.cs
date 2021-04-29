/*
-------------------------------------------------------------------------------
VPET - Virtual Production Editing Tools
vpet.research.animationsinstitut.de
https://github.com/FilmakademieRnd/VPET
 
Copyright (c) 2021 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab
 
This project has been initiated in the scope of the EU funded project 
Dreamspace (http://dreamspaceproject.eu/) under grant agreement no 610005 2014-2016.
 
Post Dreamspace the project has been further developed on behalf of the 
research and development activities of Animationsinstitut.
 
In 2018 some features (Character Animation Interface and USD support) were
addressed in the scope of the EU funded project  SAUCE (https://www.sauceproject.eu/) 
under grant agreement no 780470, 2018-2020
 
VPET consists of 3 core components: VPET Unity Client, Scene Distribution and
Syncronisation Server. They are licensed under the following terms:
-------------------------------------------------------------------------------
*/

//! @file "SceneDataDefinition.cs"
//! @brief definition of VPET scene data structure
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 26.04.2021

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

namespace vpet
{
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
        public bool editable;
        public int childCount;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] position;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] scale;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] rotation;
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
        public int geoId;
        public int textureId;
        public int materialId; // -1=standard
        public float roughness;
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
        public int bindPoseLength;
        public int rootBoneID;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] boundExtents;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] boundCenter;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16 * 99)]
        public float[] bindPoses;
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
        public LightType lightType;
        public float intensity;
        public float angle;
        public float range;
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
        public float fov;
        public float near;
        public float far;
    };

    //!
    //! Data structure for serialising the VPET header,
    //! containing scene relevant informations.
    //!
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Auto)]
    public class VpetHeader
    {
        public float lightIntensityFactor;
        public int textureBinaryType;
    }

    //!
    //! VPET structure used to hold mesh data.
    //! This sructure is used for saving to disk or sending via network.
    //!
    public class ObjectPackage
    {
        public int vSize;
        public int iSize;
        public int nSize;
        public int uvSize;
        public int bWSize;
        public Mesh mesh;
        public float[] vertices;
        public int[] indices;
        public float[] normals;
        public float[] uvs;
        public float[] boneWeights; //caution: contains 4 weights per vertex
        public int[] boneIndices;
    };

    //!
    //! VPET structure used to hold additional skinned mesh data.
    //! This sructure is used for saving to disk or sending via network.
    //!
    public class CharacterPackage
    {
        public int bMSize;
        public int sSize;
        public int rootId;
        public int[] boneMapping;
        public int[] skeletonMapping;
        public float[] bonePosition;
        public float[] boneRotation;
        public float[] boneScale;
    };

    //!
    //! VPET structure used to hold texture data.
    //! This sructure is used for saving to disk or sending via network.
    //!
    public class TexturePackage
    {
        public int colorMapDataSize;
        public int width;
        public int height;
        public TextureFormat format;
        public Texture texture;
        public byte[] colorMapData;
    };

    //!
    //! VPET structure used to hold material data.
    //! This sructure is used for saving to disk or sending via network.
    //!
    public class MaterialPackage
    {
        public int type; // 0=standard, 1=load by name, 2=new with shader by name,  3=new with shader from source, 4= .. 
        public string name;
        public string src;
        public Material mat;
    };
}

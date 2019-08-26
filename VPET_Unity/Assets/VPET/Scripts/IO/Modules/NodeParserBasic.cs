/*
-----------------------------------------------------------------------------
This source file is part of VPET - Virtual Production Editing Tool
http://vpet.research.animationsinstitut.de/
http://github.com/FilmakademieRnd/VPET

Copyright (c) 2018 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

This project has been initiated in the scope of the EU funded project 
Dreamspace under grant agreement no 610005 in the years 2014, 2015 and 2016.
http://dreamspaceproject.eu/
Post Dreamspace the project has been further developed on behalf of the 
research and development activities of Animationsinstitut.

This program is free software; you can redistribute it and/or modify it under
the terms of the MIT License as published by the Open Source Initiative.

This program is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE. See the MIT License for more details.

You should have received a copy of the MIT License along with
this program; if not go to
https://opensource.org/licenses/MIT
-----------------------------------------------------------------------------
*/
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace vpet
{

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

    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Auto)]
    public class SceneNodeGeo : SceneNode
    {
        public int geoId;
        public int textureId;
#if TRUNK
        public int materialId; // -1=standard
#endif
        public float roughness;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] color;
    };

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class SceneNodeSkinnedGeo : SceneNodeGeo
    {
        public int bindPoseLength;
        public string rootBoneDagPath;
        public float[] boundExtents;
        public float[] boundCenter;
        public float[] bindPoses;
        public string[] skinnedMeshBonesArray;
    };


    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Auto)]
    public class SceneNodeLight : SceneNode
    {
        public LightType lightType;
        public float intensity;
        public float angle;
        public float range;
        public float exposure;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public float[] color;
    };


    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Auto)]
    public class SceneNodeCam : SceneNode
    {
        public float fov;
        public float near;
        public float far;
    };


    public class NodeParserBasic
    {
        public static SceneNode ParseNode(NodeType nodeType, ref byte[] nodesByteData, int dataIdx, int length)
        {

            switch (nodeType)
            {
                case NodeType.GROUP:
                    SceneNode sceneNode = new SceneNode();
                    SceneDataHandler.ByteArrayToStruct<SceneNode>(ref nodesByteData, out sceneNode, dataIdx, length);
                    return sceneNode;
                    break;
                case NodeType.GEO:
                    SceneNodeGeo sceneNodeGeo = new SceneNodeGeo();
                    SceneDataHandler.ByteArrayToStruct<SceneNodeGeo>(ref nodesByteData, out sceneNodeGeo, dataIdx, length);
                    return sceneNodeGeo;
                    break;
                case NodeType.SKINNEDMESH:
                    SceneNodeSkinnedGeo sceneNodeSkinnedGeo = new SceneNodeSkinnedGeo();
                    SceneDataHandler.ByteArrayToStruct<SceneNodeSkinnedGeo>(ref nodesByteData, out sceneNodeSkinnedGeo, dataIdx, length);
                    return sceneNodeSkinnedGeo;
                    break;
                case NodeType.LIGHT:
                    SceneNodeLight sceneNodeLight = new SceneNodeLight();
                    SceneDataHandler.ByteArrayToStruct<SceneNodeLight>(ref nodesByteData, out sceneNodeLight, dataIdx, length);
                    return sceneNodeLight;
                    break;
                case NodeType.CAMERA:
                    SceneNodeCam sceneNodeCamera = new SceneNodeCam();
                    SceneDataHandler.ByteArrayToStruct<SceneNodeCam>(ref nodesByteData, out sceneNodeCamera, dataIdx, length);
                    return sceneNodeCamera;
                    break;
            }
            return null;
        }
    }

}

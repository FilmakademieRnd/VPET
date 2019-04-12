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
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using UnityEngine;

namespace vpet
{


    public class CacheManager : MonoBehaviour
    {

        [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Auto)]
        private class SceneNodeGeoV1100 : SceneNode
        {
            public int geoId;
            public int textureId;
            public int materialId; // -1=standard
            public float roughness;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public float[] color;
        }

        public string CacheName;
        public string CacheNameOut;

        private Version SourceVersion;
        public Version TargetVersion;

        private SceneDataHandler sceneDataHandler;

        Dictionary<string, byte[]> byteDataMap = new Dictionary<string, byte[]>();

        void Awake()
        {
            sceneDataHandler = new SceneDataHandler();
        }

        // Use this for initialization
        void Start()
        {
            SourceVersion = VPETVersion.version;

            if (CacheNameOut == null || CacheNameOut == "" || CacheNameOut == CacheName)
                CacheNameOut = CacheName + "_out";

            // Register plugins
            VPETRegister.RegisterNodeParser();
            VPETRegister.RegisterNodeBuilder();
            VPETRegister.RegisterObjectSender();



            ImportCache();

            ExportCache();
        }



        public void ImportCache()
        {

            byteDataMap.Add("header", loadBinary("header"));
            byte[] buffer = new byte[byteDataMap["header"].Length];
            Buffer.BlockCopy(byteDataMap["header"], 0, buffer, 0, byteDataMap["header"].Length);
            sceneDataHandler.HeaderByteData = buffer;

            //byteDataMap.Add("materials", loadBinary("materials"));
            byteDataMap.Add("materials", new byte[0]);
            buffer = new byte[byteDataMap["materials"].Length];
            Buffer.BlockCopy(byteDataMap["materials"], 0, buffer, 0, byteDataMap["materials"].Length);
            sceneDataHandler.MaterialsByteData = buffer;

            byteDataMap.Add("textures", loadBinary("textures"));
            buffer = new byte[byteDataMap["textures"].Length];
            Buffer.BlockCopy(byteDataMap["textures"], 0, buffer, 0, byteDataMap["textures"].Length);
            sceneDataHandler.TexturesByteData = buffer;

            byteDataMap.Add("objects", loadBinary("objects"));
            buffer = new byte[byteDataMap["objects"].Length];
            Buffer.BlockCopy(byteDataMap["objects"], 0, buffer, 0, byteDataMap["objects"].Length);
            sceneDataHandler.ObjectsByteData = buffer;
           
            byteDataMap.Add("nodes", loadBinary("nodes"));
            buffer = new byte[byteDataMap["nodes"].Length];
            Buffer.BlockCopy(byteDataMap["nodes"], 0, buffer, 0, byteDataMap["nodes"].Length);
            sceneDataHandler.NodesByteData = buffer;

        }


        private void ExportCache()
        {
            

            if (SourceVersion != TargetVersion)
                convertCache(TargetVersion);

            foreach(KeyValuePair<string, byte[]> pair in byteDataMap )
            {
                writeBinary(pair.Value, pair.Key);
            }


        }

        private void convertCache(Version currentTargetVersion)
        {
            if (currentTargetVersion == Version.V1100)
            {
                print("update nodes to version: " + currentTargetVersion);
                byteDataMap["nodes"] = getNodesByteArrayV1100();

                print("update materials to version: " + currentTargetVersion);
                // build empty material package
                byteDataMap["materials"] = new byte[0];

            }

            //if (currentTargetVersion < TargetVersion)
            //    convertCache(currentTargetVersion++);
        }


        private byte[] loadBinary(string dataname)
        {
            string filesrc = "VPET/SceneDumps/" + CacheName + "_" + dataname;
            print("Load binary data: " + filesrc);
            TextAsset asset = Resources.Load(filesrc) as TextAsset;
            return asset.bytes;
        }

        private void writeBinary(byte[] data, string dataname)
        {
            string filesrc = "Assets/Resources/VPET/SceneDumps/" + CacheNameOut + "_" + dataname + ".bytes";;
            print("Write binary data: " + filesrc);
            BinaryWriter writer = new BinaryWriter(File.Open(filesrc, FileMode.Create));
            writer.Write(data);
            writer.Close();
        }


        private byte[] getNodesByteArrayV1100()
        {
            Byte[] nodesByteData = new byte[0];
 
            foreach (SceneNode node in sceneDataHandler.NodeList)
            {
                byte[] nodeBinary;
                byte[] nodeTypeBinary;
                if (node.GetType() == typeof(SceneNodeGeo))
                {
                    nodeTypeBinary = BitConverter.GetBytes((int)NodeType.GEO);

                    SceneNodeGeo nodeGeo = (SceneNodeGeo)Convert.ChangeType(node, typeof(SceneNodeGeo));

                    // change to V1100 geo node 
                    SceneNodeGeoV1100 nodeGeoV1100 = new  SceneNodeGeoV1100();
                    copyProperties(nodeGeo, nodeGeoV1100);
                    nodeGeoV1100.materialId = -1;
                    //PrintProperties(nodeGeoV1100);
                    nodeBinary = SceneDataHandler.StructureToByteArray<SceneNodeGeoV1100>(nodeGeoV1100);
                }
                else if (node.GetType() == typeof(SceneNodeLight))
                {
                    nodeTypeBinary = BitConverter.GetBytes((int)NodeType.LIGHT);
                    SceneNodeLight nodeLight = (SceneNodeLight)Convert.ChangeType(node, typeof(SceneNodeLight));
                    nodeBinary = SceneDataHandler.StructureToByteArray<SceneNodeLight>(nodeLight);
                }
                else if (node.GetType() == typeof(SceneNodeCam))
                {
                    nodeTypeBinary = BitConverter.GetBytes((int)NodeType.CAMERA);
                    SceneNodeCam nodeCam = (SceneNodeCam)Convert.ChangeType(node, typeof(SceneNodeCam));
                    nodeBinary = SceneDataHandler.StructureToByteArray<SceneNodeCam>(nodeCam);
                }
                else if (node.GetType() == typeof(SceneNodeMocap))
                {
                    nodeTypeBinary = BitConverter.GetBytes((int)NodeType.MOCAP);
                    SceneNodeMocap nodeMocap = (SceneNodeMocap)Convert.ChangeType(node, typeof(SceneNodeMocap));
                    nodeBinary = SceneDataHandler.StructureToByteArray<SceneNodeMocap>(nodeMocap);
                }
                else
                {
                    nodeTypeBinary = BitConverter.GetBytes((int)NodeType.GROUP);
                    nodeBinary = SceneDataHandler.StructureToByteArray<SceneNode>(node);
                }
                // concate arrays
                nodesByteData = SceneDataHandler.Concat<byte>(nodesByteData, nodeTypeBinary);
                nodesByteData = SceneDataHandler.Concat<byte>(nodesByteData, nodeBinary);

            }

            return nodesByteData;
        }



        public static void copyProperties(object source, object target)
        {
            // copy property values from source class to instance 
            FieldInfo[] fis = source.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (FieldInfo info in fis)
            {
                FieldInfo fi_globals = target.GetType().GetField(info.Name, BindingFlags.Public | BindingFlags.Instance);
                if (fi_globals != null)
                {
                    fi_globals.SetValue(target, info.GetValue(source));
                }
            }
        }

        public static void PrintProperties(object source)
        {
            FieldInfo[] fis = source.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance);
            foreach (FieldInfo info in fis)
            {
                Debug.Log(string.Format("{0}: {1}", info.Name, info.GetValue(source)));
            }
        }

    }


}


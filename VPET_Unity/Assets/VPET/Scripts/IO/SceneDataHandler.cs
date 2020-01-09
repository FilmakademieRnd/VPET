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
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace vpet
{
    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Auto)]
    public class VpetHeader
    {
        public float lightIntensityFactor;
        public int textureBinaryType;
    }

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

    public class TexturePackage
    {
        public int colorMapDataSize;
        public int width;
        public int height;
        public TextureFormat format;
        public Texture texture;
        public byte[] colorMapData;
    };

    public class MaterialPackage
    {
        public int type; // 0=standard, 1=load by name, 2=new with shader by name,  3=new with shader from source, 4= .. 
        public string name;
        public string src;
        public Material mat;
    };

    public class SceneDataHandler
    {
        public static int size_int = sizeof(int);
        public static int size_float = sizeof(float);

        private List<SceneNode> m_nodeList;
        public List<SceneNode> NodeList
        {
            get { return m_nodeList; }
        }
        private List<ObjectPackage> m_objectList;
        public List<ObjectPackage> ObjectList
        {
            get { return m_objectList; }
        }
        private List<CharacterPackage> m_characterList;
        public List<CharacterPackage> CharacterList
        {
            get { return m_characterList; }
        }
        private List<TexturePackage> m_textureList;
        public List<TexturePackage> TextureList
        {
            get { return m_textureList; }
        }
        private List<MaterialPackage> m_materialList;
        public List<MaterialPackage> MaterialList
        {
            get { return m_materialList; }
        }

        private byte[] m_headerByteData;
        public byte[] HeaderByteData
        {
            set { m_headerByteData = value; convertHeaderByteStream(); }
        }

        private byte[] m_nodesByteData;
        public byte[] NodesByteData
        {
            set { m_nodesByteData = value; convertNodesByteStream(); }
        }

        private byte[] m_characterByteData;
        public byte[] CharactersByteData
        {
            set { m_characterByteData = value; convertCharacterByteStream(); }
        }

        private byte[] m_objectsByteData;
        public byte[] ObjectsByteData
        {
            set { m_objectsByteData = value; convertObjectsByteStream(); }
        }

        private byte[] m_texturesByteData;
        public byte[] TexturesByteData
        {
            set { m_texturesByteData = value; convertTexturesByteStream(); }
        }

        private byte[] m_materialsByteData;
        public byte[] MaterialsByteData
        {
            set { m_materialsByteData = value; convertMaterialsByteStream(); }
        }

        private int m_textureBinaryType = 0;
        public int TextureBinaryType
        {
            get { return m_textureBinaryType; }
        }


        public delegate SceneNode NodeParserDelegate(NodeType n, ref byte[] b, ref int o);
        public static List<NodeParserDelegate> nodeParserDelegateList = new List<NodeParserDelegate>();

        public static void RegisterDelegate(NodeParserDelegate call)
        {
            if (!nodeParserDelegateList.Contains(call))
                nodeParserDelegateList.Add(call);
        }

        public void initializeLists()
        {
            m_nodeList = new List<SceneNode>();
            m_materialList = new List<MaterialPackage>();
            m_textureList = new List<TexturePackage>();
            m_objectList = new List<ObjectPackage>();
            m_characterList = new List<CharacterPackage>();
        }

        //!
        //! function to check and reverse the endian order ( assume message from server adapter is little endian )
        //!
        public static void checkEndian(ref byte[] dataNumber)
        {
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(dataNumber);
        }


        private void convertHeaderByteStream()
        {
            int offset = 0;
            VpetHeader header = ByteArrayToStructure<VpetHeader>(ref m_headerByteData, ref offset);
            VPETSettings.Instance.lightIntensityFactor = header.lightIntensityFactor;
            VPETSettings.Instance.textureBinaryType = header.textureBinaryType;
        }

        private void convertNodesByteStream()
        {
            m_nodeList = new List<SceneNode>();

            int dataIdx = 0;
            while (dataIdx < m_nodesByteData.Length - 1)
            {
                SceneNode node = new SceneNode();

                NodeType nodeType = (NodeType)BitConverter.ToInt32(m_nodesByteData, dataIdx);
                dataIdx += size_int;

                // process all registered parse callbacks
                foreach (NodeParserDelegate nodeParserDelegate in nodeParserDelegateList)
                {
                    SceneNode _node = nodeParserDelegate(nodeType, ref m_nodesByteData, ref dataIdx);
                    if (_node != null)
                        node = _node;
                }
                //dataIdx += length;

                m_nodeList.Add(node);
            }

            Array.Clear(m_nodesByteData, 0, m_nodesByteData.Length);
            m_nodesByteData = null;
            GC.Collect();
        }

        private void convertMaterialsByteStream()
        {
            m_materialList = new List<MaterialPackage>();

            int dataIdx = 0;
            while (dataIdx < m_materialsByteData.Length - 1)
            {

                MaterialPackage matPack = new MaterialPackage();

                // get type
                int intValue = BitConverter.ToInt32(m_materialsByteData, dataIdx);
                dataIdx += size_int;
                matPack.type = intValue;

                // get material name length
                intValue = BitConverter.ToInt32(m_materialsByteData, dataIdx);
                dataIdx += size_int;

                // get material name
                byte[] nameByte = new byte[intValue];
                Buffer.BlockCopy(m_materialsByteData, dataIdx, nameByte, 0, intValue);
                dataIdx += intValue;
                matPack.name = Encoding.ASCII.GetString(nameByte);


                // get material src length
                intValue = BitConverter.ToInt32(m_materialsByteData, dataIdx);
                dataIdx += size_int;

                // get material src
                nameByte = new byte[intValue];
                Buffer.BlockCopy(m_materialsByteData, dataIdx, nameByte, 0, intValue);
                dataIdx += intValue;
                matPack.src = Encoding.ASCII.GetString(nameByte);

                m_materialList.Add(matPack);
            }

            Array.Clear(m_materialsByteData, 0, m_materialsByteData.Length);
            m_materialsByteData = null;
            GC.Collect();

        }

        private void convertTexturesByteStream()
        {
            m_textureList = new List<TexturePackage>();

            int dataIdx = 0;
            m_textureBinaryType = BitConverter.ToInt32(m_texturesByteData, dataIdx);
            dataIdx += sizeof(int);

            while (dataIdx < m_texturesByteData.Length - 1)
            {
                TexturePackage texPack = new TexturePackage();

                if (m_textureBinaryType == 1)
                {
                    int intValue = BitConverter.ToInt32(m_texturesByteData, dataIdx);
                    dataIdx += size_int;
                    texPack.width = intValue;
                    intValue = BitConverter.ToInt32(m_texturesByteData, dataIdx);
                    dataIdx += size_int;
                    texPack.height = intValue;
                    intValue = BitConverter.ToInt32(m_texturesByteData, dataIdx);
                    dataIdx += size_int;
                    texPack.format = (TextureFormat)intValue;
                }

                // pixel data
                int numValues = BitConverter.ToInt32(m_texturesByteData, dataIdx);
                dataIdx += size_int;
                texPack.colorMapDataSize = numValues;
                texPack.colorMapData = new byte[numValues];
                Buffer.BlockCopy(m_texturesByteData, dataIdx, texPack.colorMapData, 0, numValues);
                dataIdx += numValues;

                m_textureList.Add(texPack);
            }

            Array.Clear(m_texturesByteData, 0, m_texturesByteData.Length);
            m_texturesByteData = null;
            GC.Collect();
        }

        private void convertCharacterByteStream()
        {
            m_characterList = new List<CharacterPackage>();

            int dataIdx = 0;

            while (dataIdx < m_characterByteData.Length - 1)
            {
                CharacterPackage characterPack = new CharacterPackage();

                // get bone Mapping size
                characterPack.bMSize = BitConverter.ToInt32(m_characterByteData, dataIdx);
                dataIdx += size_int;

                // get bone Mapping size
                characterPack.sSize = BitConverter.ToInt32(m_characterByteData, dataIdx);
                dataIdx += size_int;

                // get root dag path
                characterPack.rootId = BitConverter.ToInt32(m_characterByteData, dataIdx);
                dataIdx += size_int;

                // get bone mapping
                characterPack.boneMapping = new int[characterPack.bMSize];
                for (int i = 0; i < characterPack.bMSize; i++)
                {
                    characterPack.boneMapping[i] = BitConverter.ToInt32(m_characterByteData, dataIdx);
                    dataIdx += size_int;
                }

                //get skeleton mapping
                characterPack.skeletonMapping = new int[characterPack.sSize];
                for (int i = 0; i < characterPack.sSize; i++)
                {
                    characterPack.skeletonMapping[i] = BitConverter.ToInt32(m_characterByteData, dataIdx);
                    dataIdx += size_int;
                }

                //get skeleton bone postions
                characterPack.bonePosition = new float[characterPack.sSize * 3];
                for (int i = 0; i < characterPack.sSize; i++)
                {
                    characterPack.bonePosition[i * 3] = BitConverter.ToSingle(m_characterByteData, dataIdx);
                    dataIdx += size_float;
                    characterPack.bonePosition[i * 3 + 1] = BitConverter.ToSingle(m_characterByteData, dataIdx);
                    dataIdx += size_float;
                    characterPack.bonePosition[i * 3 + 2] = BitConverter.ToSingle(m_characterByteData, dataIdx);
                    dataIdx += size_float;
                }

                //get skeleton bone rotations
                characterPack.boneRotation = new float[characterPack.sSize * 4];
                for (int i = 0; i < characterPack.sSize; i++)
                {
                    characterPack.boneRotation[i * 4] = BitConverter.ToSingle(m_characterByteData, dataIdx);
                    dataIdx += size_float;
                    characterPack.boneRotation[i * 4 + 1] = BitConverter.ToSingle(m_characterByteData, dataIdx);
                    dataIdx += size_float;
                    characterPack.boneRotation[i * 4 + 2] = BitConverter.ToSingle(m_characterByteData, dataIdx);
                    dataIdx += size_float;
                    characterPack.boneRotation[i * 4 + 3] = BitConverter.ToSingle(m_characterByteData, dataIdx);
                    dataIdx += size_float;
                }

                //get skeleton bone scales
                characterPack.boneScale = new float[characterPack.sSize * 3];
                for (int i = 0; i < characterPack.sSize; i++)
                {
                    characterPack.boneScale[i * 3] = BitConverter.ToSingle(m_characterByteData, dataIdx);
                    dataIdx += size_float;
                    characterPack.boneScale[i * 3 + 1] = BitConverter.ToSingle(m_characterByteData, dataIdx);
                    dataIdx += size_float;
                    characterPack.boneScale[i * 3 + 2] = BitConverter.ToSingle(m_characterByteData, dataIdx);
                    dataIdx += size_float;
                }


                m_characterList.Add(characterPack);
            }

            Array.Clear(m_objectsByteData, 0, m_objectsByteData.Length);
            m_objectsByteData = null;

            Array.Clear(m_characterByteData, 0, m_characterByteData.Length);
            m_characterByteData = null;

            GC.Collect();
        }

        private void convertObjectsByteStream()
        {
            m_objectList = new List<ObjectPackage>();

            int dataIdx = 0;
            while (dataIdx < m_objectsByteData.Length - 1)
            {
                ObjectPackage objPack = new ObjectPackage();

                // get vertices
                int numValues = BitConverter.ToInt32(m_objectsByteData, dataIdx);
                dataIdx += size_int;
                objPack.vSize = numValues;
                objPack.vertices = new float[numValues * 3];
                for (int i = 0; i < numValues * 3; i++)
                {
                    objPack.vertices[i] = BitConverter.ToSingle(m_objectsByteData, dataIdx);
                    dataIdx += size_float;
                }


                // get indices
                numValues = BitConverter.ToInt32(m_objectsByteData, dataIdx);
                dataIdx += size_int;
                objPack.iSize = numValues;
                objPack.indices = new int[numValues];
                for (int i = 0; i < numValues; i++)
                {
                    objPack.indices[i] = BitConverter.ToInt32(m_objectsByteData, dataIdx);
                    dataIdx += size_int;
                }

                // get normals
                numValues = BitConverter.ToInt32(m_objectsByteData, dataIdx);
                dataIdx += size_int;
                objPack.nSize = numValues;
                objPack.normals = new float[numValues * 3];
                for (int i = 0; i < numValues * 3; i++)
                {
                    objPack.normals[i] = BitConverter.ToSingle(m_objectsByteData, dataIdx);
                    dataIdx += size_float;
                }

                // get uvs
                numValues = BitConverter.ToInt32(m_objectsByteData, dataIdx);
                dataIdx += size_int;
                objPack.uvSize = numValues;
                objPack.uvs = new float[numValues * 2];
                for (int i = 0; i < numValues * 2; i++)
                {
                    objPack.uvs[i] = BitConverter.ToSingle(m_objectsByteData, dataIdx);
                    dataIdx += size_float;
                }

                // get boneWeights
                numValues = BitConverter.ToInt32(m_objectsByteData, dataIdx);
                dataIdx += size_int;
                objPack.bWSize = numValues;
                objPack.boneWeights = new float[numValues * 4];
                for (int i = 0; i < numValues * 4; i++)
                {
                    objPack.boneWeights[i] = BitConverter.ToSingle(m_objectsByteData, dataIdx);
                    dataIdx += size_float;
                }

                // get boneIndices
                objPack.boneIndices = new int[objPack.bWSize * 4];
                for (int i = 0; i < objPack.bWSize * 4; i++)
                {
                    objPack.boneIndices[i] = BitConverter.ToInt32(m_objectsByteData, dataIdx);
                    dataIdx += size_int;
                }

                m_objectList.Add(objPack);

            }

            // disabled because of cleanup shifted to character creation
            //Array.Clear(m_objectsByteData, 0, m_objectsByteData.Length);
            //m_objectsByteData = null;
            //GC.Collect();

        }


        //public static byte[] StructureToByteArray<T>(T obj)
        //{
        //    IntPtr ptr = IntPtr.Zero;
        //    try
        //    {
        //        int size = Marshal.SizeOf(typeof(T));
        //        ptr = Marshal.AllocHGlobal(size);
        //        Marshal.StructureToPtr(obj, ptr, true);
        //        byte[] bytes = new byte[size];
        //        Marshal.Copy(ptr, bytes, 0, size);
        //        return bytes;
        //    }
        //    finally
        //    {
        //        if (ptr != IntPtr.Zero)
        //            Marshal.FreeHGlobal(ptr);
        //    }
        //}


        public static T[] Concat<T>(T[] first, params T[][] arrays)
        {
            int length = first.Length;
            foreach (T[] array in arrays)
            {
                length += array.Length;
            }
            T[] result = new T[length];

            length = first.Length;
            Array.Copy(first, 0, result, 0, first.Length);
            foreach (T[] array in arrays)
            {
                Array.Copy(array, 0, result, length, array.Length);
                length += array.Length;
            }
            return result;
        }


        //public static T ByteArrayToStructure<T>(byte[] bytearray, ref int offset)
        //{
        //    // for debug
        //    string debugString = Encoding.ASCII.GetString(bytearray, 45, 64);
        //    Debug.Log("ByteArrayToStructure: " + debugString);
        //    IntPtr i = IntPtr.Zero;
        //    try
        //    {
        //        int len = Marshal.SizeOf(typeof(T));
        //        i = Marshal.AllocHGlobal(len);
        //        Marshal.Copy(bytearray, offset, i, len);
        //        object obj = (SceneNode)Marshal.PtrToStructure(i, typeof(T));
        //        offset += len;
        //        return (T)obj;
        //    }
        //    finally
        //    {
        //        if (i != IntPtr.Zero)
        //            Marshal.FreeHGlobal(i);
        //    }
        //}

        public static byte[] StructToByteArray<T>(T obj)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                FieldInfo[] infos = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance);
                foreach (FieldInfo info in infos)
                {
                    BinaryFormatter bf = new BinaryFormatter();
                    BinaryWriter bw = new BinaryWriter(new MemoryStream());
                    using (MemoryStream inms = new MemoryStream())
                    {
                        bf.Serialize(inms, info.GetValue(obj));
                        byte[] ba = inms.ToArray();
                        // for length
                        ms.Write(BitConverter.GetBytes(ba.Length), 0, sizeof(int));

                        // for value
                        ms.Write(ba, 0, ba.Length);
                    }
                }

                return ms.ToArray();
            }
        }

        public static T ByteArrayToStructure<T>(ref byte[] bytearray, ref int offset) where T : new()
        {
            T str = new T();

            int size = Marshal.SizeOf(str);
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.Copy(bytearray, offset, ptr, size);

            str = (T)Marshal.PtrToStructure(ptr, str.GetType());
            Marshal.FreeHGlobal(ptr);

            offset += size;

            return str;
        }

        public static byte[] StructureToByteArray(object str)
        {
            int size = Marshal.SizeOf(str);
            byte[] arr = new byte[size];
            IntPtr ptr = Marshal.AllocHGlobal(size);

            Marshal.StructureToPtr(str, ptr, true);
            Marshal.Copy(ptr, arr, 0, size);
            Marshal.FreeHGlobal(ptr);

            return arr;
        }

        public static void ByteArrayToStruct<T>(ref byte[] data, out T output, int offset, int length)
        {
            output = (T)Activator.CreateInstance(typeof(T), null);

            int debug = Marshal.SizeOf(typeof(T));
            using (MemoryStream ms = new MemoryStream(data, offset, length))
            {
                byte[] ba = null;
                FieldInfo[] infos = typeof(T).GetFields(BindingFlags.Public | BindingFlags.Instance);
                foreach (FieldInfo info in infos)
                {
                    // for length
                    ba = new byte[sizeof(int)];
                    ms.Read(ba, 0, sizeof(int));

                    // for value
                    int sz = BitConverter.ToInt32(ba, 0);
                    ba = new byte[sz];
                    ms.Read(ba, 0, sz);

                    BinaryFormatter bf = new BinaryFormatter();
                    using (MemoryStream inms = new MemoryStream(ba))
                    {
                        info.SetValue(output, bf.Deserialize(inms));
                    }
                }
            }
            offset += Marshal.SizeOf(typeof(T));
        }
    }
}
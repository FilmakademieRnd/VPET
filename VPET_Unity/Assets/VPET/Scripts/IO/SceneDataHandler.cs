using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

namespace vpet
{

    [StructLayout(LayoutKind.Sequential, Pack = 4, CharSet = CharSet.Auto)]
    public class VpetHeader
    {
        public float lightIntensityFactor = 1.0f;
        public int textureBinaryType = 1;
    }

    public class ObjectPackage
    {
        public int vSize;
        public int iSize;
        public int nSize;
        public int uvSize;
        public Mesh mesh;
        public float[] vertices;
        public int[] indices;
        public float[] normals;
        public float[] uvs;
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
        private List<TexturePackage> m_textureList;
        public List<TexturePackage> TextureList
        {
            get { return m_textureList; }
        }

        private byte[] m_nodesByteData;
        public byte[] NodesByteData
        {
            set { m_nodesByteData = value; convertNodesByteStream();  }
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


        private int m_textureBinaryType = 0;
        public int TextureBinaryType
        {
            get { return m_textureBinaryType; }
        }


        public delegate SceneNode NodeParserDelegate(NodeType n, ref byte[] b, ref int i);
        public static List<NodeParserDelegate> nodeParserDelegateList = new List<NodeParserDelegate>();

        public static void RegisterDelegate(NodeParserDelegate call)
        {
            if (!nodeParserDelegateList.Contains(call))
                nodeParserDelegateList.Add(call);
        }


        //!
        //! function to check and reverse the endian order ( assume message from server adapter is little endian )
        //!
        public static void checkEndian( ref byte[] dataNumber )
        {
            if ( !BitConverter.IsLittleEndian )
                Array.Reverse( dataNumber );
        }


        private void convertNodesByteStream()
        {
            m_nodeList = new List<SceneNode>();

            int dataIdx = 0;
            while (dataIdx < m_nodesByteData.Length - 1)
            {
                SceneNode node = new SceneNode();
                int numValues = BitConverter.ToInt32(m_nodesByteData, dataIdx);
                dataIdx += size_int;
                //checkEndian(ref sliceInt);
                NodeType nodeType = (NodeType)numValues;

                foreach(NodeParserDelegate nodeParserDelegate in nodeParserDelegateList)
                {
                    node = nodeParserDelegate(nodeType, ref m_nodesByteData, ref dataIdx);
                }

                m_nodeList.Add(node);
            }

            Array.Clear(m_nodesByteData, 0, m_nodesByteData.Length);
            m_nodesByteData = null;
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
                
                if ( m_textureBinaryType == 1)
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
                
                m_objectList.Add(objPack);

            }

            Array.Clear(m_objectsByteData, 0, m_objectsByteData.Length);
            m_objectsByteData = null;
            GC.Collect();

        }


        public static byte[] StructureToByteArray<T>(T obj)
        {
            IntPtr ptr = IntPtr.Zero;
            try
            {
                int size = Marshal.SizeOf(typeof(T));
                ptr = Marshal.AllocHGlobal(size);
                Marshal.StructureToPtr(obj, ptr, true);
                byte[] bytes = new byte[size];
                Marshal.Copy(ptr, bytes, 0, size);
                return bytes;
            }
            finally
            {
                if (ptr != IntPtr.Zero)
                    Marshal.FreeHGlobal(ptr);
            }
        }


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


        public static T ByteArrayToStructure<T>(byte[] bytearray, ref int offset)
        {
            IntPtr i = IntPtr.Zero;
            try
            {
                int len = Marshal.SizeOf(typeof(T));
                i = Marshal.AllocHGlobal(len);
                Marshal.Copy(bytearray, offset, i, len);
                object obj = (SceneNode)Marshal.PtrToStructure(i, typeof(T));
                offset += len;
                return (T)obj;
            }
            finally
            {
                if (i != IntPtr.Zero)
                    Marshal.FreeHGlobal(i);
            }
        }
    }



}
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

//! @file "SceneDataHandler.cs"
//! @brief implementation scene data deserialisation
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 23.04.2021

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;

namespace vpet
{ 
    //!
    //! Class handling scene data deserialisation.
    //!
    public class SceneDataHandler
    {
        //!
        //! storing system specific int size for faster access.
        //!
        public static int size_int = sizeof(int);
        //!
        //! storing system specific float size for faster access.
        //!
        public static int size_float = sizeof(float);

        //!
        //! The list of scne nodes for de/serialisation.
        //!
        private List<SceneNode> m_nodeList;
        //!
        //! Getter function returning a reference to the list containing 
        //! the scene nodes for de/serialisation.
        //!
        //! @return A reference to the scene nodes list.
        //!
        public ref List<SceneNode> NodeList
        {
            get { return ref m_nodeList; }
        }

        //!
        //! The list of mesh data for de/serialisation.
        //!
        private List<ObjectPackage> m_objectList;
        //!
        //! Getter function returning a reference to the list containing 
        //! the object packages (mesh data) for de/serialisation.
        //!
        //! @return A reference to the object package list.
        //!
        public ref List<ObjectPackage> ObjectList
        {
            get { return ref m_objectList; }
        }

        //!
        //! The list of skeleton data for de/serialisation.
        //!
        private List<CharacterPackage> m_characterList;
        //!
        //! Getter function returning a reference to the list containing 
        //! the character packages (skeleton data) for de/serialisation.
        //!
        //! @return A reference to the character package list.
        //!
        public ref List<CharacterPackage> CharacterList
        {
            get { return ref m_characterList; }
        }

        //!
        //! The list of texture data for de/serialisation.
        //!
        private List<TexturePackage> m_textureList;
        //!
        //! Getter function returning a reference to the list containing 
        //! the texture packages for de/serialisation.
        //!
        //! @return A reference to the texture package list.
        //!
        public ref List<TexturePackage> TextureList
        {
            get { return ref m_textureList; }
        }

        //!
        //! The list of material data for de/serialisation.
        //!
        private List<MaterialPackage> m_materialList;
        //!
        //! Getter function returning a reference to the list containing 
        //! the material packages for de/serialisation.
        //!
        //! @return A reference to the material package list.
        //!
        public ref List<MaterialPackage> MaterialList
        {
            get { return ref m_materialList; }
        }

        //!
        //! Enumeration storing texture type inforemation.
        //!
        private int m_textureBinaryType = 0;
        //!
        //! Getter function returning texture type enumeration.
        //!
        //! @return Texture type enumeration as an integer number.
        //!
        public int TextureBinaryType
        {
            get { return m_textureBinaryType; }
        }

        //!
        //! Data block to be deserialized containing header information.
        //!
        public byte[] HeaderByteData
        {
            set { convertHeaderByteStream(ref value); }
        }

        //!
        //! Data block to be deserialized containing scene node.
        //!
        public byte[] NodesByteData
        {
            set { convertNodesByteStream(ref value); }
        }

        //!
        //! Data block to be deserialized containing skeletons.
        //!
        public byte[] CharactersByteData
        {
            set { convertCharacterByteStream(ref value); }
        }

        //!
        //! Data block to be deserialized containing meshes.
        //!
        public byte[] ObjectsByteData
        {
            set { convertObjectsByteStream(ref value); }
        }

        //!
        //! Data block to be deserialized containing textures.
        //!
        public byte[] TexturesByteData
        {
            set { convertTexturesByteStream(ref value); }
        }

        //!
        //! Data block to be deserialized containing materials.
        //!
        public byte[] MaterialsByteData
        {
            set { convertMaterialsByteStream(ref value); }
        }

        //!
        //! Initialisation of the lists storing deserialised the scene data.
        //!
        public SceneDataHandler()
        {
            m_nodeList = new List<SceneNode>();
            m_materialList = new List<MaterialPackage>();
            m_textureList = new List<TexturePackage>();
            m_objectList = new List<ObjectPackage>();
            m_characterList = new List<CharacterPackage>();
        }

        //!
        //! The function deserialises a header byte stream and stores it as scene defaults.
        //!
        //! @param headerByteData The byte stream containing header data.
        //!
        private void convertHeaderByteStream(ref byte[] headerByteData)
        {
            int offset = 0;
            VpetHeader header = ByteArrayToStructure<VpetHeader>(ref headerByteData, ref offset);
            // [REVIEW]
            //VPETSettings.Instance.lightIntensityFactor = header.lightIntensityFactor;
            //VPETSettings.Instance.textureBinaryType = header.textureBinaryType;
        }

        //!
        //! The function deserialises a node byte stream and stores it into the sceneNode list.
        //!
        //! @param nodesByteData The byte stream containing scene node data.
        //!
        private void convertNodesByteStream(ref byte[] nodesByteData)
        {
            m_nodeList = new List<SceneNode>();

            int dataIdx = 0;
            while (dataIdx < nodesByteData.Length - 1)
            {
                SceneNode node = new SceneNode();

                NodeType nodeType = (NodeType) BitConverter.ToInt32(nodesByteData, dataIdx);
                dataIdx += size_int;

                switch (nodeType)
                {
                    case NodeType.GROUP:
                        node = SceneDataHandler.ByteArrayToStructure<SceneNode>(ref nodesByteData, ref dataIdx);
                        break;
                    case NodeType.GEO:
                        node = SceneDataHandler.ByteArrayToStructure<SceneNodeGeo>(ref nodesByteData, ref dataIdx);
                        break;
                    case NodeType.SKINNEDMESH:
                        node = SceneDataHandler.ByteArrayToStructure<SceneNodeSkinnedGeo>(ref nodesByteData, ref dataIdx);
                        break;
                    case NodeType.LIGHT:
                        node = SceneDataHandler.ByteArrayToStructure<SceneNodeLight>(ref nodesByteData, ref dataIdx);
                        break;
                    case NodeType.CAMERA:
                        node = SceneDataHandler.ByteArrayToStructure<SceneNodeCam>(ref nodesByteData, ref dataIdx);
                        break;
                }

                m_nodeList.Add(node);
            }
        }

        //!
        //! The function deserialises a material byte stream and stores it into the material list.
        //!
        //! @param materialsByteData The byte stream containing material data.
        //!
        private void convertMaterialsByteStream(ref byte[] materialsByteData)
        {
            m_materialList = new List<MaterialPackage>();

            int dataIdx = 0;
            while (dataIdx < materialsByteData.Length - 1)
            {

                MaterialPackage matPack = new MaterialPackage();

                // get type
                int intValue = BitConverter.ToInt32(materialsByteData, dataIdx);
                dataIdx += size_int;
                matPack.type = intValue;

                // get material name length
                intValue = BitConverter.ToInt32(materialsByteData, dataIdx);
                dataIdx += size_int;

                // get material name
                byte[] nameByte = new byte[intValue];
                Buffer.BlockCopy(materialsByteData, dataIdx, nameByte, 0, intValue);
                dataIdx += intValue;
                matPack.name = Encoding.ASCII.GetString(nameByte);


                // get material src length
                intValue = BitConverter.ToInt32(materialsByteData, dataIdx);
                dataIdx += size_int;

                // get material src
                nameByte = new byte[intValue];
                Buffer.BlockCopy(materialsByteData, dataIdx, nameByte, 0, intValue);
                dataIdx += intValue;
                matPack.src = Encoding.ASCII.GetString(nameByte);

                m_materialList.Add(matPack);
            }
        }

        //!
        //! The function deserialises a texture byte stream and stores it into the texture list.
        //!
        //! @param texturesByteData The byte stream containing texture data.
        //!
        private void convertTexturesByteStream(ref byte[] texturesByteData)
        {
            m_textureList = new List<TexturePackage>();

            int dataIdx = 0;
            m_textureBinaryType = BitConverter.ToInt32(texturesByteData, dataIdx);
            dataIdx += sizeof(int);

            while (dataIdx < texturesByteData.Length - 1)
            {
                TexturePackage texPack = new TexturePackage();

                if (m_textureBinaryType == 1)
                {
                    int intValue = BitConverter.ToInt32(texturesByteData, dataIdx);
                    dataIdx += size_int;
                    texPack.width = intValue;
                    intValue = BitConverter.ToInt32(texturesByteData, dataIdx);
                    dataIdx += size_int;
                    texPack.height = intValue;
                    intValue = BitConverter.ToInt32(texturesByteData, dataIdx);
                    dataIdx += size_int;
                    texPack.format = (TextureFormat)intValue;
                }

                // pixel data
                int numValues = BitConverter.ToInt32(texturesByteData, dataIdx);
                dataIdx += size_int;
                texPack.colorMapDataSize = numValues;
                texPack.colorMapData = new byte[numValues];
                Buffer.BlockCopy(texturesByteData, dataIdx, texPack.colorMapData, 0, numValues);
                dataIdx += numValues;

                m_textureList.Add(texPack);
            }
        }

        //[REVIEW]
        //!
        //! The function deserialises a character byte stream and stores it into the character list.
        //!
        //! @param characterByteData The byte stream containing character data.
        //!
        private void convertCharacterByteStream(ref byte[] characterByteData)
        {
            m_characterList = new List<CharacterPackage>();

            int dataIdx = 0;

            while (dataIdx < characterByteData.Length - 1)
            {
                CharacterPackage characterPack = new CharacterPackage();

                // get bone Mapping size
                characterPack.bMSize = BitConverter.ToInt32(characterByteData, dataIdx);
                dataIdx += size_int;

                // get bone Mapping size
                characterPack.sSize = BitConverter.ToInt32(characterByteData, dataIdx);
                dataIdx += size_int;

                // get root dag path
                characterPack.rootId = BitConverter.ToInt32(characterByteData, dataIdx);
                dataIdx += size_int;

                // get bone mapping
                characterPack.boneMapping = new int[characterPack.bMSize];
                for (int i = 0; i < characterPack.bMSize; i++)
                {
                    characterPack.boneMapping[i] = BitConverter.ToInt32(characterByteData, dataIdx);
                    dataIdx += size_int;
                }

                //get skeleton mapping
                characterPack.skeletonMapping = new int[characterPack.sSize];
                for (int i = 0; i < characterPack.sSize; i++)
                {
                    characterPack.skeletonMapping[i] = BitConverter.ToInt32(characterByteData, dataIdx);
                    dataIdx += size_int;
                }

                //get skeleton bone postions
                characterPack.bonePosition = new float[characterPack.sSize * 3];
                for (int i = 0; i < characterPack.sSize; i++)
                {
                    characterPack.bonePosition[i * 3] = BitConverter.ToSingle(characterByteData, dataIdx);
                    dataIdx += size_float;
                    characterPack.bonePosition[i * 3 + 1] = BitConverter.ToSingle(characterByteData, dataIdx);
                    dataIdx += size_float;
                    characterPack.bonePosition[i * 3 + 2] = BitConverter.ToSingle(characterByteData, dataIdx);
                    dataIdx += size_float;
                }

                //get skeleton bone rotations
                characterPack.boneRotation = new float[characterPack.sSize * 4];
                for (int i = 0; i < characterPack.sSize; i++)
                {
                    characterPack.boneRotation[i * 4] = BitConverter.ToSingle(characterByteData, dataIdx);
                    dataIdx += size_float;
                    characterPack.boneRotation[i * 4 + 1] = BitConverter.ToSingle(characterByteData, dataIdx);
                    dataIdx += size_float;
                    characterPack.boneRotation[i * 4 + 2] = BitConverter.ToSingle(characterByteData, dataIdx);
                    dataIdx += size_float;
                    characterPack.boneRotation[i * 4 + 3] = BitConverter.ToSingle(characterByteData, dataIdx);
                    dataIdx += size_float;
                }

                //get skeleton bone scales
                characterPack.boneScale = new float[characterPack.sSize * 3];
                for (int i = 0; i < characterPack.sSize; i++)
                {
                    characterPack.boneScale[i * 3] = BitConverter.ToSingle(characterByteData, dataIdx);
                    dataIdx += size_float;
                    characterPack.boneScale[i * 3 + 1] = BitConverter.ToSingle(characterByteData, dataIdx);
                    dataIdx += size_float;
                    characterPack.boneScale[i * 3 + 2] = BitConverter.ToSingle(characterByteData, dataIdx);
                    dataIdx += size_float;
                }


                m_characterList.Add(characterPack);
            }
        }

        //!
        //! The function deserialises a mesh byte stream and stores it into the object list.
        //!
        //! @param objectsByteData The byte stream containing mesh data.
        //!
        private void convertObjectsByteStream(ref byte[] objectsByteData)
        {
            m_objectList = new List<ObjectPackage>();

            int dataIdx = 0;
            while (dataIdx < objectsByteData.Length - 1)
            {
                ObjectPackage objPack = new ObjectPackage();

                // get vertices
                int numValues = BitConverter.ToInt32(objectsByteData, dataIdx);
                dataIdx += size_int;
                objPack.vSize = numValues;
                objPack.vertices = new float[numValues * 3];
                for (int i = 0; i < numValues * 3; i++)
                {
                    objPack.vertices[i] = BitConverter.ToSingle(objectsByteData, dataIdx);
                    dataIdx += size_float;
                }


                // get indices
                numValues = BitConverter.ToInt32(objectsByteData, dataIdx);
                dataIdx += size_int;
                objPack.iSize = numValues;
                objPack.indices = new int[numValues];
                for (int i = 0; i < numValues; i++)
                {
                    objPack.indices[i] = BitConverter.ToInt32(objectsByteData, dataIdx);
                    dataIdx += size_int;
                }

                // get normals
                numValues = BitConverter.ToInt32(objectsByteData, dataIdx);
                dataIdx += size_int;
                objPack.nSize = numValues;
                objPack.normals = new float[numValues * 3];
                for (int i = 0; i < numValues * 3; i++)
                {
                    objPack.normals[i] = BitConverter.ToSingle(objectsByteData, dataIdx);
                    dataIdx += size_float;
                }

                // get uvs
                numValues = BitConverter.ToInt32(objectsByteData, dataIdx);
                dataIdx += size_int;
                objPack.uvSize = numValues;
                objPack.uvs = new float[numValues * 2];
                for (int i = 0; i < numValues * 2; i++)
                {
                    objPack.uvs[i] = BitConverter.ToSingle(objectsByteData, dataIdx);
                    dataIdx += size_float;
                }

                // get boneWeights
                numValues = BitConverter.ToInt32(objectsByteData, dataIdx);
                dataIdx += size_int;
                objPack.bWSize = numValues;
                objPack.boneWeights = new float[numValues * 4];
                for (int i = 0; i < numValues * 4; i++)
                {
                    objPack.boneWeights[i] = BitConverter.ToSingle(objectsByteData, dataIdx);
                    dataIdx += size_float;
                }

                // get boneIndices
                objPack.boneIndices = new int[objPack.bWSize * 4];
                for (int i = 0; i < objPack.bWSize * 4; i++)
                {
                    objPack.boneIndices[i] = BitConverter.ToInt32(objectsByteData, dataIdx);
                    dataIdx += size_int;
                }

                m_objectList.Add(objPack);
            }
        }

        //! 
        //! Template function for deserialising byte streams into arbitrary structures. 
        //! 
        //! @param bytearray The byte stream to be deserialised.
        //! @param offset The offset in bytes used to interate over the array.
        //! 
        private static T ByteArrayToStructure<T>(ref byte[] bytearray, ref int offset) where T : new()
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
    }
}

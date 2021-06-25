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
under grant agreement no 780470, 2018-2021
 
VPET consists of 3 core components: VPET Unity Client, Scene Distribution and
Syncronisation Server. They are licensed under the following terms:
-------------------------------------------------------------------------------
*/

//! @file "SceneDataHandler.cs"
//! @brief implementation scene data deserialisation
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 23.06.2021

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
    public partial class SceneManager : Manager
    {
        public class SceneDataHandler
        {
            //!
            //! Class for storing VPET scene object information.
            //!
            public class SceneData
            {
                public VpetHeader header;
                public List<SceneNode> nodeList;
                public List<ObjectPackage> objectList;
                public List<CharacterPackage> characterList;
                public List<TexturePackage> textureList;
                public List<MaterialPackage> materialList;

                public SceneData()
                {
                    header = new VpetHeader();
                    nodeList = new List<SceneNode>();
                    objectList = new List<ObjectPackage>();
                    characterList = new List<CharacterPackage>();
                    textureList = new List<TexturePackage>();
                    materialList = new List<MaterialPackage>();
                }
            }

            //!
            //! storing system specific int size for faster access.
            //!
            public static int size_int = sizeof(int);
            //!
            //! storing system specific float size for faster access.
            //!
            public static int size_float = sizeof(float);
            //!
            //! The list containing the serialised header.
            //!
            private byte[] m_headerByteData;
            //!
            //! Getter function returning a reference to the byte array  
            //! containing the serialised header data.
            //!
            //! @return A reference to the serialised header data.
            //!
            public ref byte[] headerByteDataRef
            {
                get { return ref m_headerByteData; }
            }
            //!
            //! Setter function for setting a the byte array,
            //! containing the serialised header data.
            //!
            public byte[] headerByteData
            {
                set { m_headerByteData = value; }
            }

            //!
            //! The list containing the serialised nodes.
            //!
            private byte[] m_nodesByteData;
            //!
            //! Getter function returning a reference to the byte array  
            //! containing the serialised nodes data.
            //!
            //! @return A reference to the serialised nodes data.
            //!
            public ref byte[] nodesByteDataRef
            {
                get { return ref m_nodesByteData; }
            }
            //!
            //! Setter function for setting a the byte array,
            //! containing the serialised nodes data.
            //!
            public byte[] nodesByteData
            {
                set { m_nodesByteData = value; }
            }
            //!
            //! The list containing the serialised meshes.
            //!
            private byte[] m_objectsByteData;
            //!
            //! Getter function returning a reference to the byte array  
            //! containing the serialised objects data.
            //!
            //! @return A reference to the serialised objects data.
            //!
            public ref byte[] objectsByteDataRef
            {
                get { return ref m_objectsByteData; }
            }
            //!
            //! Setter function for setting a the byte array,
            //! containing the serialised object data.
            //!
            public byte[] objectsByteData
            {
                set { m_objectsByteData = value; }
            }
            //!
            //! The list containing the serialised skinned meshes.
            //!
            private byte[] m_characterByteData;
            //!
            //! Getter function returning a reference to the byte array  
            //! containing the serialised characters data.
            //!
            //! @return A reference to the serialised characters data.
            //!
            public ref byte[] characterByteDataRef
            {
                get { return ref m_characterByteData; }
            }
            //!
            //! Setter function for setting a the byte array,
            //! containing the serialised skinned mesh data.
            //!
            public byte[] characterByteData
            {
                set { m_characterByteData = value; }
            }
            //!
            //! The list containing the serialised textures.
            //!
            private byte[] m_texturesByteData;
            //!
            //! Getter function returning a reference to the byte array  
            //! containing the serialised textures data.
            //!
            //! @return A reference to the serialised textures data.
            //!
            public ref byte[] texturesByteDataRef
            {
                get { return ref m_texturesByteData; }
            }
            //!
            //! Setter function for setting a the byte array,
            //! containing the serialised texture data.
            //!
            public byte[] texturesByteData
            {
                set { m_texturesByteData = value; }
            }
            //!
            //! The list containing the serialised materials.
            //!
            private byte[] m_materialsByteData;
            //!
            //! Getter function returning a reference to the byte array  
            //! containing the serialised materials data.
            //!
            //! @return A reference to the serialised materials data.
            //!
            public ref byte[] materialsByteDataRef
            {
                get { return ref m_materialsByteData; }
            }
            //!
            //! Setter function for setting a the byte array,
            //! containing the serialised material data.
            //!
            public byte[] materialsByteData
            {
                set { m_materialsByteData = value; }
            }
            //!
            //! Initialisation of the lists storing deserialised the scene data.
            //!
            public SceneDataHandler()
            {
            }

            //!
            //! Deletes all scene byte data stored in the SceneDataHandler.
            //!
            public void clearSceneByteData()
            {
                m_headerByteData = new byte[0];
                m_nodesByteData = new byte[0];
                m_objectsByteData = new byte[0];
                m_characterByteData = new byte[0]; 
                m_texturesByteData = new byte[0];
                m_materialsByteData = new byte[0];
            }

            //!
            //! Function to convert byte arrays into VPET SceneData.
            //! Make shure to set the respective byte arrays before calling this function!
            //!
            public SceneData getSceneData()
            {
                SceneData sceneData = new SceneData();

                if (m_headerByteData != null && m_headerByteData.Length > 0)
                    sceneData.header = convertHeaderByteStream();
                else  
                    Helpers.Log("SceneDataHandler: Header byte array null or empty!", Helpers.logMsgType.WARNING);

                if (m_nodesByteData != null && m_nodesByteData.Length > 0)
                    sceneData.nodeList = convertNodesByteStream();
                else
                    Helpers.Log("SceneDataHandler: Nodes byte array null or empty!", Helpers.logMsgType.WARNING);

                if (m_objectsByteData != null && m_objectsByteData.Length > 0)
                    sceneData.objectList = convertObjectsByteStream();
                else
                    Helpers.Log("SceneDataHandler: Objects byte array null or empty!", Helpers.logMsgType.WARNING);

                if (m_characterByteData != null && m_characterByteData.Length > 0)
                    sceneData.characterList = convertCharacterByteStream();
                else
                    Helpers.Log("SceneDataHandler: Character byte array null or empty!", Helpers.logMsgType.WARNING);

                if (m_texturesByteData != null && m_texturesByteData.Length > 0)
                    sceneData.textureList = convertTexturesByteStream(ref sceneData);
                else
                    Helpers.Log("SceneDataHandler: Textures byte array null or empty!", Helpers.logMsgType.WARNING);

                if (m_materialsByteData != null && m_materialsByteData.Length > 0)
                    sceneData.materialList = convertMaterialsByteStream();
                else
                    Helpers.Log("SceneDataHandler: Materieal byte array null or empty!", Helpers.logMsgType.WARNING);

                // [REVIEW]
                //VPETSettings.Instance.lightIntensityFactor = header.lightIntensityFactor;
                //VPETSettings.Instance.textureBinaryType = header.textureBinaryType;

                return sceneData;
            }

            //!
            //! Function to convert SceneData into byte arrays.
            //! SceneData will be deleted during this process!
            //! 
            //! @param sceneData the scruct containing the VPET scene data.
            //!
            public void setSceneData(ref SceneData sceneData)
            {
                // create byte arrays and clear buffers
                m_headerByteData = StructureToByteArray(sceneData.header);

                getNodesByteArray(ref sceneData.nodeList);
                sceneData.nodeList.Clear();

                getObjectsByteArray(ref sceneData.objectList);
                sceneData.objectList.Clear();

                getCharacterByteArray(ref sceneData.characterList);
                sceneData.characterList.Clear();

                getTexturesByteArray(ref sceneData.textureList);
                sceneData.textureList.Clear();

                getMaterialsByteArray(ref sceneData.materialList);
                sceneData.materialList.Clear();
            }

            //!
            //! The function deserialises a header byte stream and stores it as scene defaults.
            //!
            private VpetHeader convertHeaderByteStream()
            {
                int offset = 0;
                return ByteArrayToStructure<VpetHeader>(ref m_headerByteData, ref offset);
            }

            //!
            //! The function deserialises a node byte stream and stores it into the sceneNode list.
            //!
            private List<SceneNode> convertNodesByteStream()
            {
                List<SceneNode> nodeList = new List<SceneNode>();

                int dataIdx = 0;
                while (dataIdx < m_nodesByteData.Length - 1)
                {
                    SceneNode node = new SceneNode();

                    NodeType nodeType = (NodeType)BitConverter.ToInt32(m_nodesByteData, dataIdx);
                    dataIdx += size_int;

                    switch (nodeType)
                    {
                        case NodeType.GROUP:
                            node = SceneDataHandler.ByteArrayToStructure<SceneNode>(ref m_nodesByteData, ref dataIdx);
                            break;
                        case NodeType.GEO:
                            node = SceneDataHandler.ByteArrayToStructure<SceneNodeGeo>(ref m_nodesByteData, ref dataIdx);
                            break;
                        case NodeType.SKINNEDMESH:
                            node = SceneDataHandler.ByteArrayToStructure<SceneNodeSkinnedGeo>(ref m_nodesByteData, ref dataIdx);
                            break;
                        case NodeType.LIGHT:
                            node = SceneDataHandler.ByteArrayToStructure<SceneNodeLight>(ref m_nodesByteData, ref dataIdx);
                            break;
                        case NodeType.CAMERA:
                            node = SceneDataHandler.ByteArrayToStructure<SceneNodeCam>(ref m_nodesByteData, ref dataIdx);
                            break;
                    }

                    nodeList.Add(node);
                }
                return nodeList;
            }

            //!
            //! The function deserialises a material byte stream and stores it into the material list.
            //!
            private List<MaterialPackage> convertMaterialsByteStream()
            {
                List<MaterialPackage> materialList = new List<MaterialPackage>();

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

                    materialList.Add(matPack);
                }
                return materialList;
            }

            //!
            //! The function deserialises a texture byte stream and stores it into the texture list.
            //!
            //! @param sceneData The scene data for checking the tyture type.
            //!
            private List<TexturePackage> convertTexturesByteStream(ref SceneData sceneData)
            {
                List<TexturePackage> textureList = new List<TexturePackage>();
                int dataIdx = 0;

                while (dataIdx < m_texturesByteData.Length - 1)
                {
                    TexturePackage texPack = new TexturePackage();

                    if (sceneData.header.textureBinaryType == 1)
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

                    textureList.Add(texPack);
                }
                return textureList;
            }

            //!
            //! The function deserialises a character byte stream and stores it into the character list.
            //!
            private List<CharacterPackage> convertCharacterByteStream()
            {
                List<CharacterPackage> characterList = new List<CharacterPackage>();

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

                    characterList.Add(characterPack);
                }
                return characterList;
            }

            //!
            //! The function deserialises a mesh byte stream and stores it into the object list.
            //!
            private List<ObjectPackage> convertObjectsByteStream()
            {
                List<ObjectPackage> objectList = new List<ObjectPackage>();

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

                    objectList.Add(objPack);
                }
                return objectList;
            }

            //!
            //! Function that concatinates all serialised VPET nodes to a byte array.
            //!
            private void getNodesByteArray(ref List<SceneManager.SceneNode> nodeList)
            {
                m_nodesByteData = new byte[0];
                foreach (SceneManager.SceneNode node in nodeList)
                {
                    byte[] nodeBinary;
                    byte[] nodeTypeBinary;
                    if (node.GetType() == typeof(SceneManager.SceneNodeGeo))
                    {
                        nodeTypeBinary = BitConverter.GetBytes((int)NodeType.GEO);
                        SceneManager.SceneNodeGeo nodeGeo = (SceneManager.SceneNodeGeo)Convert.ChangeType(node, typeof(SceneManager.SceneNodeGeo));
                        nodeBinary = StructureToByteArray(nodeGeo);
                    }
                    else if (node.GetType() == typeof(SceneManager.SceneNodeSkinnedGeo))
                    {
                        nodeTypeBinary = BitConverter.GetBytes((int)NodeType.SKINNEDMESH);
                        SceneManager.SceneNodeSkinnedGeo nodeskinnedGeo = (SceneManager.SceneNodeSkinnedGeo)Convert.ChangeType(node, typeof(SceneManager.SceneNodeSkinnedGeo));
                        nodeBinary = StructureToByteArray(nodeskinnedGeo);
                    }
                    else if (node.GetType() == typeof(SceneManager.SceneNodeLight))
                    {
                        nodeTypeBinary = BitConverter.GetBytes((int)NodeType.LIGHT);
                        SceneManager.SceneNodeLight nodeLight = (SceneManager.SceneNodeLight)Convert.ChangeType(node, typeof(SceneManager.SceneNodeLight));
                        nodeBinary = StructureToByteArray(nodeLight);
                    }
                    else if (node.GetType() == typeof(SceneManager.SceneNodeCam))
                    {
                        nodeTypeBinary = BitConverter.GetBytes((int)NodeType.CAMERA);
                        SceneManager.SceneNodeCam nodeCam = (SceneManager.SceneNodeCam)Convert.ChangeType(node, typeof(SceneManager.SceneNodeCam));
                        nodeBinary = StructureToByteArray(nodeCam);
                    }
                    else
                    {
                        nodeTypeBinary = BitConverter.GetBytes((int)NodeType.GROUP);
                        nodeBinary = StructureToByteArray(node);
                    }

                    // concate arrays
                    m_nodesByteData = Concat<byte>(m_nodesByteData, nodeTypeBinary);
                    m_nodesByteData = Concat<byte>(m_nodesByteData, nodeBinary);
                }
            }

            //!
            //! Function that concatinates all serialised VPET meshes to a byte array.
            //!
            //! @param objectList The list that contains the serialised meshes to be concatinated.
            //!
            private void getObjectsByteArray(ref List<ObjectPackage> objectList)
            {
                m_objectsByteData = new byte[0];

                foreach (ObjectPackage objPack in objectList)
                {
                    byte[] objByteData = new byte[5 * SceneDataHandler.size_int +
                                                        objPack.vSize * 3 * SceneDataHandler.size_float +
                                                        objPack.iSize * SceneDataHandler.size_int +
                                                        objPack.nSize * 3 * SceneDataHandler.size_float +
                                                        objPack.uvSize * 2 * SceneDataHandler.size_float +
                                                        objPack.bWSize * 4 * SceneDataHandler.size_float +
                                                        objPack.bWSize * 4 * SceneDataHandler.size_int];
                    int dstIdx = 0;
                    // vertices
                    Buffer.BlockCopy(BitConverter.GetBytes(objPack.vSize), 0, objByteData, dstIdx, SceneDataHandler.size_int);
                    dstIdx += SceneDataHandler.size_int;
                    Buffer.BlockCopy(objPack.vertices, 0, objByteData, dstIdx, objPack.vSize * 3 * SceneDataHandler.size_float);
                    dstIdx += objPack.vSize * 3 * SceneDataHandler.size_float;
                    // indices
                    Buffer.BlockCopy(BitConverter.GetBytes(objPack.iSize), 0, objByteData, dstIdx, SceneDataHandler.size_int);
                    dstIdx += SceneDataHandler.size_int;
                    Buffer.BlockCopy(objPack.indices, 0, objByteData, dstIdx, objPack.iSize * SceneDataHandler.size_int);
                    dstIdx += objPack.iSize * SceneDataHandler.size_int;
                    // normals
                    Buffer.BlockCopy(BitConverter.GetBytes(objPack.nSize), 0, objByteData, dstIdx, SceneDataHandler.size_int);
                    dstIdx += SceneDataHandler.size_int;
                    Buffer.BlockCopy(objPack.normals, 0, objByteData, dstIdx, objPack.nSize * 3 * SceneDataHandler.size_float);
                    dstIdx += objPack.nSize * 3 * SceneDataHandler.size_float;
                    // uvs
                    Buffer.BlockCopy(BitConverter.GetBytes(objPack.uvSize), 0, objByteData, dstIdx, SceneDataHandler.size_int);
                    dstIdx += SceneDataHandler.size_int;
                    Buffer.BlockCopy(objPack.uvs, 0, objByteData, dstIdx, objPack.uvSize * 2 * SceneDataHandler.size_float);
                    dstIdx += objPack.uvSize * 2 * SceneDataHandler.size_float;
                    // bone weights
                    Buffer.BlockCopy(BitConverter.GetBytes(objPack.bWSize), 0, objByteData, dstIdx, SceneDataHandler.size_int);
                    dstIdx += SceneDataHandler.size_int;
                    Buffer.BlockCopy(objPack.boneWeights, 0, objByteData, dstIdx, objPack.bWSize * 4 * SceneDataHandler.size_float);
                    dstIdx += objPack.bWSize * 4 * SceneDataHandler.size_float;
                    // bone indices
                    Buffer.BlockCopy(objPack.boneIndices, 0, objByteData, dstIdx, objPack.bWSize * 4 * SceneDataHandler.size_int);
                    dstIdx += objPack.bWSize * 4 * SceneDataHandler.size_int;

                    // concate
                    m_objectsByteData = Concat<byte>(m_objectsByteData, objByteData);
                }
            }

            //!
            //! Function that concatinates all serialised VPET skinned meshes to a byte array.
            //!
            //! @param characterList The list that contains the serialised skinned meshes to be concatinated.
            //!
            private void getCharacterByteArray(ref List<CharacterPackage> characterList)
            {
                m_characterByteData = new byte[0];
                foreach (CharacterPackage chrPack in characterList)
                {
                    byte[] characterByteData = new byte[SceneDataHandler.size_int * 3 +
                                                    chrPack.boneMapping.Length * SceneDataHandler.size_int +
                                                    chrPack.skeletonMapping.Length * SceneDataHandler.size_int +
                                                    chrPack.sSize * SceneDataHandler.size_float * 10];
                    int dstIdx = 0;
                    // bone mapping size
                    Buffer.BlockCopy(BitConverter.GetBytes(chrPack.bMSize), 0, characterByteData, dstIdx, SceneDataHandler.size_int);
                    dstIdx += SceneDataHandler.size_int;

                    // skeleton mapping size
                    Buffer.BlockCopy(BitConverter.GetBytes(chrPack.sSize), 0, characterByteData, dstIdx, SceneDataHandler.size_int);
                    dstIdx += SceneDataHandler.size_int;

                    // root dag id
                    Buffer.BlockCopy(BitConverter.GetBytes(chrPack.rootId), 0, characterByteData, dstIdx, SceneDataHandler.size_int);
                    dstIdx += SceneDataHandler.size_int;

                    Buffer.BlockCopy(chrPack.boneMapping, 0, characterByteData, dstIdx, chrPack.bMSize * SceneDataHandler.size_int);
                    dstIdx += chrPack.bMSize * SceneDataHandler.size_int;

                    // skeleton Mapping
                    Buffer.BlockCopy(chrPack.skeletonMapping, 0, characterByteData, dstIdx, chrPack.sSize * SceneDataHandler.size_int);
                    dstIdx += chrPack.sSize * SceneDataHandler.size_int;

                    //skelton bone positions
                    Buffer.BlockCopy(chrPack.bonePosition, 0, characterByteData, dstIdx, chrPack.sSize * 3 * SceneDataHandler.size_float);
                    dstIdx += chrPack.sSize * 3 * SceneDataHandler.size_float;

                    //skelton bone rotations
                    Buffer.BlockCopy(chrPack.boneRotation, 0, characterByteData, dstIdx, chrPack.sSize * 4 * SceneDataHandler.size_float);
                    dstIdx += chrPack.sSize * 4 * SceneDataHandler.size_float;

                    //skelton bone scales
                    Buffer.BlockCopy(chrPack.boneScale, 0, characterByteData, dstIdx, chrPack.sSize * 3 * SceneDataHandler.size_float);
                    dstIdx += chrPack.sSize * 3 * SceneDataHandler.size_float;

                    // concate
                    m_characterByteData = Concat<byte>(m_characterByteData, characterByteData);
                }
            }

            //!
            //! Function that concatinates all serialised VPET textures to a byte array.
            //!
            //! @param textureList The list that contains the serialised textures to be concatinated.
            //!
            private void getTexturesByteArray(ref List<TexturePackage> textureList)
            {
                m_texturesByteData = new byte[0];

                foreach (TexturePackage texPack in textureList)
                {
                    byte[] texByteData = new byte[4 * SceneDataHandler.size_int + texPack.colorMapDataSize];
                    int dstIdx = 0;
                    // width
                    Buffer.BlockCopy(BitConverter.GetBytes(texPack.width), 0, texByteData, dstIdx, SceneDataHandler.size_int);
                    dstIdx += SceneDataHandler.size_int;
                    // height
                    Buffer.BlockCopy(BitConverter.GetBytes(texPack.height), 0, texByteData, dstIdx, SceneDataHandler.size_int);
                    dstIdx += SceneDataHandler.size_int;
                    // format
                    Buffer.BlockCopy(BitConverter.GetBytes((int)texPack.format), 0, texByteData, dstIdx, SceneDataHandler.size_int);
                    dstIdx += SceneDataHandler.size_int;
                    // pixel data
                    Buffer.BlockCopy(BitConverter.GetBytes(texPack.colorMapDataSize), 0, texByteData, dstIdx, SceneDataHandler.size_int);
                    dstIdx += SceneDataHandler.size_int;
                    Buffer.BlockCopy(texPack.colorMapData, 0, texByteData, dstIdx, texPack.colorMapDataSize);
                    dstIdx += texPack.colorMapDataSize;

                    // concate
                    m_texturesByteData = Concat<byte>(m_texturesByteData, texByteData);
                }
            }

            //!
            //! Function that concatinates all serialised VPET materials to a byte array.
            //!
            //! @param materialList The list that contains the serialised materials to be concatinated.
            //!
            private void getMaterialsByteArray(ref List<MaterialPackage> materialList)
            {
                m_materialsByteData = new byte[0];

                foreach (MaterialPackage matPack in materialList)
                {
                    byte[] matByteData = new byte[SceneDataHandler.size_int + SceneDataHandler.size_int + matPack.name.Length + SceneDataHandler.size_int + matPack.src.Length];
                    int dstIdx = 0;

                    // type
                    Buffer.BlockCopy(BitConverter.GetBytes(matPack.type), 0, matByteData, dstIdx, SceneDataHandler.size_int);
                    dstIdx += SceneDataHandler.size_int;

                    // name length
                    Buffer.BlockCopy(BitConverter.GetBytes(matPack.name.Length), 0, matByteData, dstIdx, SceneDataHandler.size_int);
                    dstIdx += SceneDataHandler.size_int;

                    // name
                    byte[] nameByte = Encoding.ASCII.GetBytes(matPack.name);
                    Buffer.BlockCopy(nameByte, 0, matByteData, dstIdx, matPack.name.Length);
                    dstIdx += matPack.name.Length;

                    // src length
                    Buffer.BlockCopy(BitConverter.GetBytes(matPack.src.Length), 0, matByteData, dstIdx, SceneDataHandler.size_int);
                    dstIdx += SceneDataHandler.size_int;

                    // src
                    nameByte = Encoding.ASCII.GetBytes(matPack.src);
                    Buffer.BlockCopy(nameByte, 0, matByteData, dstIdx, matPack.src.Length);
                    dstIdx += matPack.src.Length;

                    // concate
                    m_materialsByteData = Concat<byte>(m_materialsByteData, matByteData);
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

            //!
            //! Template function for concatination of arrays. 
            //!
            //! @param first The array field to be appended to.
            //! @param arrays The arrays to be append.
            //!
            private static T[] Concat<T>(T[] first, params T[][] arrays)
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

            //! 
            //! Template function for serialising arbitrary structures in to byte streams.
            //! 
            //! @param obj The object to be serialised.
            //!
            private static byte[] StructureToByteArray(object obj)
            {
                int size = Marshal.SizeOf(obj);
                byte[] arr = new byte[size];
                IntPtr ptr = Marshal.AllocHGlobal(size);

                Marshal.StructureToPtr(obj, ptr, true);
                Marshal.Copy(ptr, arr, 0, size);
                Marshal.FreeHGlobal(ptr);

                return arr;
            }
        }
    }
}

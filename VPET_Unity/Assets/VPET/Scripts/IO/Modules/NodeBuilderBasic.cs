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
using System;
using System.Text;
using UnityEngine;
using System.Reflection;

namespace vpet
{

    public enum LightTypeKatana { disk, directional, sphere, rect }

    public class NodeBuilderBasic
    {
        private static int idCount = 0;

        public static GameObject BuildNode(ref SceneNode node, Transform parent, GameObject obj, bool resetID, ref List<Tuple<Renderer, string, string[]>> skinnedMeshRootBones)
        {
            if (resetID)
                idCount = 0;
            if (node.GetType() == typeof(SceneNodeGeo))
            {
                SceneNodeGeo nodeGeo = (SceneNodeGeo)Convert.ChangeType(node, typeof(SceneNodeGeo));
                return CreateObject(nodeGeo, parent);
            }
            else if (node.GetType() == typeof(SceneNodeSkinnedGeo))
            {
                SceneNodeSkinnedGeo nodeSkinnedGeo = (SceneNodeSkinnedGeo)Convert.ChangeType(node, typeof(SceneNodeSkinnedGeo));
                return CreateSkinnedObject(nodeSkinnedGeo, parent, ref skinnedMeshRootBones);
            }
            else if (node.GetType() == typeof(SceneNodeLight))
            {
                SceneNodeLight nodeLight = (SceneNodeLight)Convert.ChangeType(node, typeof(SceneNodeLight));
                GameObject _obj = CreateLight(nodeLight, parent);
                SceneLoader.SelectableLights.Add(_obj);
                return _obj;
            }
            else if (node.GetType() == typeof(SceneNodeCam))
            {
                SceneNodeCam nodeCam = (SceneNodeCam)Convert.ChangeType(node, typeof(SceneNodeCam));
                // make the camera editable
                // nodeCam.editable = true;
                return CreateCamera(nodeCam, parent);
            }
            else if (node.GetType() == typeof(SceneNode))
            {
                return CreateNode(node, parent);
            }


            return null;

        }



        //!
        //! function create the object from mesh data
        //! @param  scnObjKtn   object which holds the data
        //!
        public static GameObject CreateNode(SceneNode node, Transform parentTransform)
        {
            GameObject objMain;
            // Tranform
            Vector3 pos = new Vector3(node.position[0], node.position[1], node.position[2]);
            //print( "Position: " + pos );
            Quaternion rot = new Quaternion(node.rotation[0], node.rotation[1], node.rotation[2], node.rotation[3]);
            // Vector3 euler = rot.eulerAngles;
            //print( "Euler: " + euler );
            //rot = new Quaternion();
            //Vector3 axis = new Vector3(0, 180, 0);
            //rot.eulerAngles = euler+axis;
            Vector3 scl = new Vector3(node.scale[0], node.scale[1], node.scale[2]);
            //print( "Scale: " + scl );

            if (!parentTransform.Find(Encoding.ASCII.GetString(node.name)))
            {
                // set up object basics
                objMain = new GameObject();
                objMain.name = Encoding.ASCII.GetString(node.name);

                //place object
                objMain.transform.parent = parentTransform; // GameObject.Find( "Scene" ).transform;
            }
            else
            {
                objMain = parentTransform.Find(Encoding.ASCII.GetString(node.name)).gameObject;
            }

            objMain.transform.localPosition = pos; // new Vector3( 0, 0, 0 );
            objMain.transform.localRotation = rot; //  Quaternion.identity;
            objMain.transform.localScale = scl; // new Vector3( 1, 1, 1 );
            objMain.layer = 0;

            if (node.editable)
            {
                SceneObject sceneObject = objMain.AddComponent<SceneObject>();
                sceneObject.id = idCount++;
            }

            return objMain;
        }

        //!
        //! function to create the object from mesh data for non-skinned meshes
        //! @param  scnObjKtn   object which holds the data
        //!
        public static GameObject CreateObject(SceneNodeGeo nodeGeo, Transform parentTransform)
        {
            GameObject objMain;

            // Transform / convert handiness
            Vector3 pos = new Vector3(nodeGeo.position[0], nodeGeo.position[1], nodeGeo.position[2]);

            // Rotation / convert handiness
            Quaternion rot = new Quaternion(nodeGeo.rotation[0], nodeGeo.rotation[1], nodeGeo.rotation[2], nodeGeo.rotation[3]);

            // Scale
            Vector3 scl = new Vector3(nodeGeo.scale[0], nodeGeo.scale[1], nodeGeo.scale[2]);

            if (!parentTransform.Find(Encoding.ASCII.GetString(nodeGeo.name)))
            {
                // Material Properties and Textures
                Material mat;
#if TRUNK
                // assign material from material list
                if (nodeGeo.materialId > -1 && nodeGeo.materialId < SceneLoader.SceneMaterialList.Count)
                {
                    mat = SceneLoader.SceneMaterialList[nodeGeo.materialId];
                }
                else // or set standard
                {
                    mat = new Material(Shader.Find("Standard"));
                }
#else
                mat = new Material( Shader.Find( "Standard" ) );
#endif

                // map properties
                if (VPETSettings.Instance.doLoadTextures)
                {
                    SceneLoader.MapMaterialProperties(mat, nodeGeo);
                }

                // set up object basics
                objMain = new GameObject();
                objMain.name = Encoding.ASCII.GetString(nodeGeo.name);

                // Add Material
                Renderer renderer;
                renderer = objMain.AddComponent<MeshRenderer>();

                renderer.material = mat;

                // Add Mesh
                if (nodeGeo.geoId > -1 && nodeGeo.geoId < SceneLoader.SceneMeshList.Count)
                {
                    Mesh[] meshes = SceneLoader.SceneMeshList[nodeGeo.geoId];

                    objMain.AddComponent<MeshFilter>();
                    objMain.GetComponent<MeshFilter>().mesh = meshes[0];
                    VPETSettings.Instance.sceneBoundsMax = Vector3.Max(VPETSettings.Instance.sceneBoundsMax, renderer.bounds.max);
                    VPETSettings.Instance.sceneBoundsMin = Vector3.Min(VPETSettings.Instance.sceneBoundsMin, renderer.bounds.min);

                    for (int i = 1; i < meshes.Length; i++)
                    {
                        GameObject subObj = new GameObject(objMain.name + "_part" + i.ToString());

                        Renderer subRenderer;
                        subRenderer = subObj.AddComponent<MeshRenderer>();
                        subObj.AddComponent<MeshFilter>();
                        subObj.GetComponent<MeshFilter>().mesh = meshes[i];

                        subRenderer.material = mat;
                        subObj.transform.parent = objMain.transform;
                        VPETSettings.Instance.sceneBoundsMax = Vector3.Max(VPETSettings.Instance.sceneBoundsMax, subRenderer.bounds.max);
                        VPETSettings.Instance.sceneBoundsMin = Vector3.Min(VPETSettings.Instance.sceneBoundsMin, subRenderer.bounds.min);
                    }

                    Vector3 sceneExtends = VPETSettings.Instance.sceneBoundsMax - VPETSettings.Instance.sceneBoundsMin;
                    VPETSettings.Instance.maxExtend = Mathf.Max(Mathf.Max(sceneExtends.x, sceneExtends.y), sceneExtends.z);
                }

                //place object
                objMain.transform.parent = parentTransform; // GameObject.Find( "Scene" ).transform;
            }
            else
            {
                objMain = parentTransform.Find(Encoding.ASCII.GetString(nodeGeo.name)).gameObject;
            }

            objMain.transform.localPosition = pos; // new Vector3( 0, 0, 0 );
            objMain.transform.localRotation = rot; //  Quaternion.identity;
            objMain.transform.localScale = scl; // new Vector3( 1, 1, 1 );

            if (nodeGeo.editable)
            {
                SceneObject sceneObject = objMain.AddComponent<SceneObject>();
                sceneObject.id = idCount++;
            }

            return objMain;

        }

        //!
        //! function to create the object from mesh data for skinned meshes
        //! @param  scnObjKtn   object which holds the data
        //!
        public static GameObject CreateSkinnedObject(SceneNodeSkinnedGeo nodeGeo, Transform parentTransform, ref List<Tuple<Renderer, string, string[]>> skinnedMeshRootBones)
        {
            GameObject objMain;

            // Transform / convert handiness
            Vector3 pos = new Vector3(nodeGeo.position[0], nodeGeo.position[1], nodeGeo.position[2]);

            // Rotation / convert handiness
            Quaternion rot = new Quaternion(nodeGeo.rotation[0], nodeGeo.rotation[1], nodeGeo.rotation[2], nodeGeo.rotation[3]);

            // Scale
            Vector3 scl = new Vector3(nodeGeo.scale[0], nodeGeo.scale[1], nodeGeo.scale[2]);

            if (!parentTransform.Find(Encoding.ASCII.GetString(nodeGeo.name)))
            {
                // Material Properties and Textures
                Material mat;
#if TRUNK
                // assign material from material list
                if (nodeGeo.materialId > -1 && nodeGeo.materialId < SceneLoader.SceneMaterialList.Count)
                {
                    mat = SceneLoader.SceneMaterialList[nodeGeo.materialId];
                }
                else // or set standard
                {
                    mat = new Material(Shader.Find("Standard"));
                }
#else
                mat = new Material( Shader.Find( "Standard" ) );
#endif

                // map properties
                if (VPETSettings.Instance.doLoadTextures)
                {
                    SceneLoader.MapMaterialProperties(mat, nodeGeo);
                }

                // set up object basics
                objMain = new GameObject();
                objMain.name = Encoding.ASCII.GetString(nodeGeo.name);

                // Add Material
                Renderer renderer;
                renderer = objMain.AddComponent<SkinnedMeshRenderer>();

                // Add Mesh
                if (nodeGeo.geoId > -1 && nodeGeo.geoId < SceneLoader.SceneMeshList.Count)
                {
                    Mesh[] meshes = SceneLoader.SceneMeshList[nodeGeo.geoId];

                    SkinnedMeshRenderer sRenderer = ((SkinnedMeshRenderer)renderer);

                    string rootBoneDagPath = nodeGeo.rootBoneDagPath;

                    skinnedMeshRootBones.Add(new Tuple<Renderer, string, string[]>(renderer, rootBoneDagPath, nodeGeo.skinnedMeshBonesArray));
                    VPETSettings.Instance.sceneBoundsMax = Vector3.Max(VPETSettings.Instance.sceneBoundsMax, renderer.bounds.max);
                    VPETSettings.Instance.sceneBoundsMin = Vector3.Min(VPETSettings.Instance.sceneBoundsMin, renderer.bounds.min);
                    Bounds bounds = new Bounds(new Vector3(nodeGeo.boundCenter[0], nodeGeo.boundCenter[1], nodeGeo.boundCenter[2]),
                                                new Vector3(nodeGeo.boundExtents[0] * 2f, nodeGeo.boundExtents[1] * 2f, nodeGeo.boundExtents[2] * 2f));
                    sRenderer.localBounds = bounds;

                    Matrix4x4[] bindposes = new Matrix4x4[nodeGeo.bindPoseLength];
                    for (int i = 0; i < nodeGeo.bindPoseLength; i++)
                    {
                        bindposes[i] = new Matrix4x4();
                        bindposes[i][0, 0] = nodeGeo.bindPoses[i * 16];
                        bindposes[i][0, 1] = nodeGeo.bindPoses[i * 16 + 1];
                        bindposes[i][0, 2] = nodeGeo.bindPoses[i * 16 + 2];
                        bindposes[i][0, 3] = nodeGeo.bindPoses[i * 16 + 3];
                        bindposes[i][1, 0] = nodeGeo.bindPoses[i * 16 + 4];
                        bindposes[i][1, 1] = nodeGeo.bindPoses[i * 16 + 5];
                        bindposes[i][1, 2] = nodeGeo.bindPoses[i * 16 + 6];
                        bindposes[i][1, 3] = nodeGeo.bindPoses[i * 16 + 7];
                        bindposes[i][2, 0] = nodeGeo.bindPoses[i * 16 + 8];
                        bindposes[i][2, 1] = nodeGeo.bindPoses[i * 16 + 9];
                        bindposes[i][2, 2] = nodeGeo.bindPoses[i * 16 + 10];
                        bindposes[i][2, 3] = nodeGeo.bindPoses[i * 16 + 11];
                        bindposes[i][3, 0] = nodeGeo.bindPoses[i * 16 + 12];
                        bindposes[i][3, 1] = nodeGeo.bindPoses[i * 16 + 13];
                        bindposes[i][3, 2] = nodeGeo.bindPoses[i * 16 + 14];
                        bindposes[i][3, 3] = nodeGeo.bindPoses[i * 16 + 15];
                    }
                    meshes[0].bindposes = bindposes;
                    sRenderer.sharedMesh = meshes[0];
                    sRenderer.material = mat;

                    for (int i = 1; i < meshes.Length; i++)
                    {
                        GameObject subObj = new GameObject(objMain.name + "_part" + i.ToString());

                        SkinnedMeshRenderer subRenderer;
                        subRenderer = subObj.AddComponent<SkinnedMeshRenderer>();

                        rootBoneDagPath = nodeGeo.rootBoneDagPath;
                        skinnedMeshRootBones.Add(new Tuple<Renderer, string, string[]>(subRenderer, rootBoneDagPath, nodeGeo.skinnedMeshBonesArray));
                        Matrix4x4[] subBindposes = new Matrix4x4[nodeGeo.bindPoseLength];
                        for (int j = 0; j < nodeGeo.bindPoseLength; j++)
                        {
                            subBindposes[j] = new Matrix4x4();
                            subBindposes[j].m00 = nodeGeo.bindPoses[j * 16];
                            subBindposes[j].m01 = nodeGeo.bindPoses[j * 16 + 1];
                            subBindposes[j].m02 = nodeGeo.bindPoses[j * 16 + 2];
                            subBindposes[j].m03 = nodeGeo.bindPoses[j * 16 + 3];
                            subBindposes[j].m10 = nodeGeo.bindPoses[j * 16 + 4];
                            subBindposes[j].m11 = nodeGeo.bindPoses[j * 16 + 5];
                            subBindposes[j].m12 = nodeGeo.bindPoses[j * 16 + 6];
                            subBindposes[j].m13 = nodeGeo.bindPoses[j * 16 + 7];
                            subBindposes[j].m20 = nodeGeo.bindPoses[j * 16 + 8];
                            subBindposes[j].m21 = nodeGeo.bindPoses[j * 16 + 9];
                            subBindposes[j].m22 = nodeGeo.bindPoses[j * 16 + 10];
                            subBindposes[j].m23 = nodeGeo.bindPoses[j * 16 + 11];
                            subBindposes[j].m30 = nodeGeo.bindPoses[j * 16 + 12];
                            subBindposes[j].m31 = nodeGeo.bindPoses[j * 16 + 13];
                            subBindposes[j].m32 = nodeGeo.bindPoses[j * 16 + 14];
                            subBindposes[j].m33 = nodeGeo.bindPoses[j * 16 + 15];
                        }
                        meshes[i].bindposes = bindposes;
                        subRenderer.sharedMesh = meshes[i];

                        subRenderer.material = mat;
                        subObj.transform.parent = objMain.transform;
                        VPETSettings.Instance.sceneBoundsMax = Vector3.Max(VPETSettings.Instance.sceneBoundsMax, subRenderer.bounds.max);
                        VPETSettings.Instance.sceneBoundsMin = Vector3.Min(VPETSettings.Instance.sceneBoundsMin, subRenderer.bounds.min);
                        Bounds subBounds = new Bounds(new Vector3(nodeGeo.boundCenter[0], nodeGeo.boundCenter[1], nodeGeo.boundCenter[2]),
                                                      new Vector3(nodeGeo.boundExtents[0], nodeGeo.boundExtents[1], nodeGeo.boundExtents[2]));
                        subRenderer.localBounds = bounds;
                    }

                    Vector3 sceneExtends = VPETSettings.Instance.sceneBoundsMax - VPETSettings.Instance.sceneBoundsMin;
                    VPETSettings.Instance.maxExtend = Mathf.Max(Mathf.Max(sceneExtends.x, sceneExtends.y), sceneExtends.z);
                }

                //place object back at original position
                objMain.transform.parent = parentTransform;
            }
            else
            {
                objMain = parentTransform.Find(Encoding.ASCII.GetString(nodeGeo.name)).gameObject;
            }

            objMain.transform.localPosition = pos; // new Vector3( 0, 0, 0 );
            objMain.transform.localRotation = rot; //  Quaternion.identity;
            objMain.transform.localScale = scl; // new Vector3( 1, 1, 1 );

            if (nodeGeo.editable)
            {
                SceneObject sceneObject = objMain.AddComponent<SceneObject>();
                sceneObject.id = idCount++;
            }

            return objMain;

        }



        //!
        //! function create the object from mesh data
        //! @param  node   object which holds the data
        //! @param  parentTransform   parent object
        //!
        public static GameObject CreateLight(SceneNodeLight nodeLight, Transform parentTransform)
        {

            // Tranform
            Vector3 pos = new Vector3(nodeLight.position[0], nodeLight.position[1], nodeLight.position[2]);
            Quaternion rot = new Quaternion(nodeLight.rotation[0], nodeLight.rotation[1], nodeLight.rotation[2], nodeLight.rotation[3]);
            Vector3 scl = new Vector3(nodeLight.scale[0], nodeLight.scale[1], nodeLight.scale[2]);

            // set up object basics
            GameObject objMain = new GameObject();
            objMain.name = Encoding.ASCII.GetString(nodeLight.name);

            //place object
            objMain.transform.SetParent(parentTransform, false);
            objMain.transform.localPosition = pos;
            objMain.transform.localRotation = rot;
            objMain.transform.localScale = Vector3.one;

            // Add light prefab
            GameObject lightUber = Resources.Load<GameObject>("VPET/Prefabs/UberLight");
            GameObject _lightUberInstance = GameObject.Instantiate(lightUber);
            _lightUberInstance.name = lightUber.name;
            lightUber.transform.GetChild(0).gameObject.layer = 8;


            Light lightComponent = _lightUberInstance.GetComponent<Light>();
            // instert type here!!!
            lightComponent.type = nodeLight.lightType;
            lightComponent.color = new Color(nodeLight.color[0], nodeLight.color[1], nodeLight.color[2]);
            lightComponent.intensity = nodeLight.intensity * VPETSettings.Instance.lightIntensityFactor;
            lightComponent.spotAngle = nodeLight.angle;
            if (lightComponent.type == LightType.Directional)
            {
                lightComponent.shadows = LightShadows.Soft;
                lightComponent.shadowStrength = 0.8f;
            }
            else
                lightComponent.shadows = LightShadows.None;
            lightComponent.shadowBias = 0f;
            lightComponent.shadowNormalBias = 1f;
            lightComponent.range = nodeLight.range * VPETSettings.Instance.sceneScale;

            Debug.Log("Create Light: " + nodeLight.name + " of type: " + ((LightTypeKatana)(nodeLight.lightType)).ToString() + " Intensity: " + nodeLight.intensity + " Pos: " + pos);

            // Add light specific settings
            if (nodeLight.lightType == LightType.Directional)
            {
            }
            else if (nodeLight.lightType == LightType.Spot)
            {
                lightComponent.range *= 2;
                //objMain.transform.Rotate(new Vector3(0, 180f, 0), Space.Self);
            }
            else if (nodeLight.lightType == LightType.Area)
            {
                // TODO: use are lights when supported in unity
                lightComponent.spotAngle = 170;
                lightComponent.range *= 4;
            }
            else
            {
                Debug.Log("Unknown Light Type in NodeBuilderBasic::CreateLight");
            }


            // parent 
            _lightUberInstance.transform.SetParent(objMain.transform, false);

            // add scene object for interactivity at the light quad
            //if (nodeLight.editable)
            //{
            SceneObjectLight sco = objMain.AddComponent<SceneObjectLight>();
            sco.id = idCount++;
            sco.exposure = nodeLight.exposure;
            //}

            // Rotate 180 around y-axis because lights and cameras have additional eye space coordinate system
            //objMain.transform.Rotate(new Vector3(0, 180f, 0), Space.Self);

            // TODO: what for ??
            objMain.layer = 0;

            return objMain;
        }

        //!
        //! function create the object from mesh data
        //! @param  node   object which holds the data
        //! @param  parentTransform   parent object
        //!
        public static GameObject CreateCamera(SceneNodeCam nodeCam, Transform parentTransform)
        {
            // Tranform
            Vector3 pos = new Vector3(nodeCam.position[0], nodeCam.position[1], nodeCam.position[2]);
            Quaternion rot = new Quaternion(nodeCam.rotation[0], nodeCam.rotation[1], nodeCam.rotation[2], nodeCam.rotation[3]);
            Vector3 scl = new Vector3(nodeCam.scale[0], nodeCam.scale[1], nodeCam.scale[2]);

            // set up object basics
            GameObject objMain = new GameObject();
            objMain.name = Encoding.ASCII.GetString(nodeCam.name);

            // add camera dummy mesh
            GameObject cameraObject = Resources.Load<GameObject>("VPET/Prefabs/cameraObject");
            GameObject cameraInstance = GameObject.Instantiate(cameraObject);
            cameraInstance.SetActive(false);
            cameraInstance.name = cameraObject.name;
            cameraInstance.transform.SetParent(objMain.transform, false);
            cameraInstance.transform.localScale = new Vector3(1, 1, 1) * VPETSettings.Instance.sceneScale * 20.0f;
            cameraInstance.transform.localPosition = new Vector3(0, 0, 0);
            cameraInstance.transform.localRotation = Quaternion.AngleAxis(180, Vector3.up);

            // add camera data script and set values
            //if (nodeCam.editable)
            //{
            SceneObjectCamera sco = objMain.AddComponent<SceneObjectCamera>();
            sco.id = idCount++;
            sco.fov = nodeCam.fov;
            sco.near = nodeCam.near;
            sco.far = nodeCam.far;
            //}

            // place camera
            objMain.transform.parent = parentTransform;
            objMain.transform.localPosition = pos;
            objMain.transform.localRotation = rot;
            objMain.transform.localScale = scl;

            // Rotate 180 around y-axis because lights and cameras have additional eye space coordinate system
            //objMain.transform.Rotate(new Vector3(0, 180f, 0), Space.Self);

            // TODO: what for ??
            objMain.layer = 0;

            // add to list for later access as camera location
            SceneLoader.SceneCameraList.Add(objMain);

            return objMain;
        }
    }

}

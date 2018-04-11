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
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

//!
//! Script creating the scene received from the server (providing XML3D) within Unity 
//!
namespace vpet
{
	
	public class SceneLoader : MonoBehaviour 
	{
	    //!
	    //! name of the parent gameobject, all objects go underneath it
	    //!
	    public string sceneParent = "Scene";
	
	
	    private GameObject scnPrtGO;
	    
		public static List<Texture2D> SceneTextureList = new List<Texture2D>();
		public static List<Mesh[]> SceneMeshList = new List<Mesh[]>();
        public static List<GameObject> SceneEditableObjects = new List<GameObject>();
		public static List<GameObject> SelectableLights = new List<GameObject>();														  
	    public static List<GameObject> SceneCameraList = new List<GameObject>();

		public static GameObject scnRoot;												

        private List<GameObject> geometryPassiveList = new List<GameObject>();
        private SceneDataHandler sceneDataHandler;
        
		public SceneDataHandler SceneDataHandler
        {
            get { return sceneDataHandler; }
        }
	
        public delegate GameObject NodeBuilderDelegate(ref SceneNode n, Transform t, GameObject o);
        public static List<NodeBuilderDelegate> nodeBuilderDelegateList = new List<NodeBuilderDelegate>();

        public static void RegisterDelegate(NodeBuilderDelegate call)
        {
            if (!nodeBuilderDelegateList.Contains(call))
                nodeBuilderDelegateList.Add(call);
        }


        void Awake()
        {
            sceneDataHandler = new SceneDataHandler();
        }

        void Start()
	    {
	        // create scene parent if not there
	        scnPrtGO = GameObject.Find( sceneParent );
	        if ( scnPrtGO == null )
	        {
	            scnPrtGO = new GameObject( sceneParent );
	        }

            scnRoot = scnPrtGO.transform.Find("root").gameObject;
	        if (scnRoot == null)
	        {
	            scnRoot = new GameObject("root");
	            scnRoot.transform.parent = scnPrtGO.transform;
	        }
	    }	

        public void ResetScene()
        {
            SceneEditableObjects.Clear();
            SceneTextureList.Clear();
            SceneMeshList.Clear();
            SceneCameraList.Clear();
            geometryPassiveList.Clear();


            if (scnRoot != null)
            {
                foreach (Transform child in scnRoot.transform)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }            
        }

        public void createSceneGraph( )
	    {

            print("lightIntensityFactor : " + VPETSettings.Instance.lightIntensityFactor);
            print("textureBinaryType : " + VPETSettings.Instance.textureBinaryType);

            // create textures
            if (VPETSettings.Instance.doLoadTextures )
	        {
	            createTextures();
	        }      
	
	        // create meshes
	        createMeshes();
	
	        // iterate nodes
	        createSceneGraphIter(scnRoot.transform, 0);
	
	        // make editable        
	        foreach ( GameObject g in SceneEditableObjects )
	        {
	            SceneObject sobj = g.GetComponent<SceneObject>();
	            if ( sobj == null )
	            {
	                g.AddComponent<SceneObject>();
	            }
	        }

            // set default orthographic size
            Camera.main.orthographicSize = 1000 * VPETSettings.Instance.sceneScale;

	        //Transform geoTransform = GameObject.Find( "Main Camera" ).transform;
	        //if ( geoTransform != null )
	        //{
	        //    print( "Set geo transform" );
	        //    JoystickInput joystickScript = GameObject.Find( "JoystickAdapter" ).gameObject.GetComponent<JoystickInput>();
	        //    joystickScript.WorldTransform = geoTransform;        
	        //}
        }

        public bool isEditable(GameObject targetObj)
        {
            foreach( GameObject g in SceneEditableObjects)
            {
                if (g == targetObj)
                    return true;
            }
            return false;
        }
	
	
	    private int createSceneGraphIter( Transform parent, int idx  )
	    {
	        GameObject obj = null; // = new GameObject( scnObjKtn.rawNodeList[idx].name );
	        
	        SceneNode node = sceneDataHandler.NodeList[idx];
	
			// process all registered build callbacks
			foreach(NodeBuilderDelegate nodeBuilderDelegate in nodeBuilderDelegateList)
			{
				GameObject _obj = nodeBuilderDelegate(ref node, parent, obj);
				if (_obj != null)
					obj = _obj;
			}

            // add scene object to editable 
	        if ( node.editable )
	        {
	            SceneEditableObjects.Add( obj );
	        }
	
            // recursive call
	        int idxChild = idx;
	        for ( int k = 1; k <= node.childCount; k++ )
	        {
	            idxChild = createSceneGraphIter( obj.transform, idxChild+1 );
	        }
	
	        return idxChild;
	    }
	
	
	    private void createTextures()
	    {
            foreach ( TexturePackage texPack in sceneDataHandler.TextureList )
	        {
                if (sceneDataHandler.TextureBinaryType == 1)
                {
                    Texture2D tex_2d = new Texture2D(texPack.width, texPack.height, texPack.format, false);
                    tex_2d.LoadRawTextureData(texPack.colorMapData);
                    tex_2d.Apply();
                    SceneTextureList.Add(tex_2d);
                }
                else
                {
#if UNITY_IPHONE
                    Texture2D tex_2d = new Texture2D(16, 16, TextureFormat.PVRTC_RGBA4, false);
#else
                    Texture2D tex_2d = new Texture2D(16, 16, TextureFormat.DXT5Crunched, false);
#endif
                    tex_2d.LoadImage(texPack.colorMapData);
                    SceneTextureList.Add(tex_2d);
                }

            }
	    }
	
	
	    //!
		//! function ??
	    //! @param  ??   ??
	    //!    
	    private void createMeshes( )
	    {

            foreach (ObjectPackage objPack in sceneDataHandler.ObjectList)
            {
                Vector3[] vertices = new Vector3[objPack.vSize];
                Vector3[] normals = new Vector3[objPack.nSize];
                Vector2[] uv = new Vector2[objPack.uvSize];


                // TODO Handiness, see below!?
                for (int i = 0; i < objPack.vSize; i++)
                {
                    Vector3 v = new Vector3(objPack.vertices[i * 3 + 0], objPack.vertices[i * 3 + 1], objPack.vertices[i * 3 + 2]);
                    vertices[i] = v;
                }

                for (int i = 0; i < objPack.nSize; i++)
                {
                    Vector3 v = new Vector3(objPack.normals[i * 3 + 0], objPack.normals[i * 3 + 1], objPack.normals[i * 3 + 2]);
                    normals[i] = v;
                }

                for (int i = 0; i < objPack.uvSize; i++)
                {
                    Vector2 v2 = new Vector2(objPack.uvs[i * 2 + 0], objPack.uvs[i * 2 + 1]);
                    uv[i] = v2;
                }

                createSplitMesh(vertices, normals, uv, objPack.indices);
            }
            /*
            for ( int k = 0; k<sceneDataHandler.ObjectList; k++ )
	        {
	
	            // vertices
	            Vector3[] verts = new Vector3[scnObjKtn.rawVertexList[k].Length/3];
	            for ( int i = 0; i < verts.Length; i++ )
	            {
	                // convert handiness
	                verts[i] = new Vector3( -scnObjKtn.rawVertexList[k][i * 3], scnObjKtn.rawVertexList[k][i * 3 + 1], scnObjKtn.rawVertexList[k][i * 3 + 2] );
	            }
	
	            // uvs Vector2 per vertex point
	            Vector2[] uvs = new Vector2[verts.Length];
	            for ( int i = 0; i < verts.Length; i++ )
	            {
	                uvs[i] = new Vector2( scnObjKtn.rawUvList[k][i * 2], scnObjKtn.rawUvList[k][i * 2 + 1] );
	            }
	
	            // normals Vector3 per vertex point
	            Vector3[] norms = new Vector3[verts.Length];
	            for ( int i = 0; i < verts.Length; i++ )
	            {
	                // convert handiness
	                norms[i] = new Vector3( -scnObjKtn.rawNormalList[k][i * 3], scnObjKtn.rawNormalList[k][i * 3 + 1], scnObjKtn.rawNormalList[k][i * 3 + 2] );
	            }
	
	            // Triangles
	            int[] tris = new int[scnObjKtn.rawIndexList[k].Length];
	            tris = scnObjKtn.rawIndexList[k];
	
	            //print( " verts length " + verts.Length );
	
	        }
	        */
	    }
	
	

	
	    //! function creating game objects and build hierarchy identical to dagpath
	    //! @param  nodes           node array to the leaf object including the leaf object
	    //! @param  dagpathPrefix   where to place in existing scene hierarchy
	    //! @return                 Transform leaf parent
	    private Transform createHierarchy( string[] nodes, string dagpathPrefix="/" )
	    {
	        Transform parentTransform = null;
	        GameObject parentGO = GameObject.Find( dagpathPrefix );
	        if ( parentGO )
	            parentTransform = parentGO.transform;
	        int idx = 0;
	        while( idx <= nodes.Length-2 )
	        {
	            string path = dagpathPrefix + string.Join( "/", nodes, 0, idx+1 );
	            GameObject g = GameObject.Find( path );
	            if ( g == null )
	            {
	                g = new GameObject( nodes[idx] );
	                g.transform.parent = parentTransform;
	            }
	            parentTransform = g.transform;
	            idx++;
	        }
	
	        return parentTransform;
	    }
	
	
	
	    //! function create mesh at the given gameobject and split if necessary
	    //! @param  obj             gameobject to work on
	    //! @param  vertices        
	    //! @param  normals        
	    //! @param  uvs        
	    //! @param  triangles        
	    //! @param  material        
	    private void createSplitMesh( Vector3[] vertices, Vector3[] normals, Vector2[] uvs, int[] triangles )
	    {
	
	        List<Mesh> meshList = new List<Mesh>();
	
	        // TODO: review
	        // if more than 65K vertices, split the mesh in submeshs
	        if ( vertices.Length > 65000 )
	        {
	            // print( String.Format( "Split object: {0}", obj.name ) );
	
	            int triIndex = 0;
	            int triIndexMax = triangles.Length-1;
	            int triIndexOffset = 63000;
	            int subObjCount = 0;
	
	            while ( triIndex < triIndexMax )
	            {
	                subObjCount++;
	
	                List<Vector3> subVertices = new List<Vector3>();
	                List<Vector3> subNormals = new List<Vector3>();
	                List<Vector2> subUVs = new List<Vector2>();
	                List<int> subTriangles = new List<int>();
	
	                int[] mapVertexIndices = new int[vertices.Length];
	                for ( int i = 0; i<mapVertexIndices.Length; i++ )
	                {
	                    mapVertexIndices[i] = -1;
	                }
	
	                for ( int i = 0; i<triIndexOffset; i++ )
	                {
	                    int idx = triangles[triIndex + i];
	                    if ( mapVertexIndices[idx] != -1 )
	                    {
	                        subTriangles.Add( mapVertexIndices[idx] );
	                    }
	                    else
	                    {
	                        subVertices.Add( vertices[idx] );
	                        subNormals.Add( normals[idx] );
	                        subUVs.Add( uvs[idx] );
	                        subTriangles.Add( subVertices.Count-1 );
	                        mapVertexIndices[idx] = subVertices.Count-1;
	                    }
	                }
	
	
	                // print( "create :" + subObj.name + " triIndex " + triIndex  + " triOffset " + triIndexOffset + " numVertices " + subVertices.Count);
	
	                Mesh mesh = new Mesh(); 
	                mesh.Clear();
	                mesh.vertices = subVertices.ToArray();
	                mesh.normals = subNormals.ToArray();
	                mesh.uv = subUVs.ToArray();
	                mesh.triangles = subTriangles.ToArray();
	                meshList.Add( mesh );
	
	
	                triIndex += triIndexOffset;
	
	                if ( triIndex+triIndexOffset > triIndexMax )
	                {
	                    triIndexOffset = triIndexMax-triIndex+1;
	                }
	
	            }
	        }
	        else
	        {
	            Mesh mesh = new Mesh(); 
	            mesh.Clear();
	            mesh.vertices = vertices;
	            mesh.normals = normals;
	            mesh.uv = uvs;
	            mesh.triangles = triangles;
	            meshList.Add( mesh );
	            // mesh.RecalculateNormals();
	            // mesh.RecalculateBounds();
	        }
	
	        SceneMeshList.Add( meshList.ToArray() );
	    }
	
        public void HideGeometry()
        {
            if (geometryPassiveList.Count == 0 && scnRoot.transform.childCount > 0 )
            {
                getGeometryIter(scnRoot.transform);
            }

            foreach (GameObject g in geometryPassiveList)
            {
                g.SetActive(false);
            }
        }

        public void ShowGeometry()
        {
            foreach (GameObject g in geometryPassiveList)
            {
                g.SetActive(true);
            }
        }

        private void getGeometryIter(Transform t)
        {
            if (t.GetComponentInChildren<SceneObject>() == null && t.GetComponentInChildren<Light>() == null && t.GetComponentInChildren<Camera>() == null)
            {
                geometryPassiveList.Add(t.gameObject);
            }
            else if (t.GetComponent<SceneObject>() == null && t.GetComponent<Light>() == null && t.GetComponent<Camera>() == null)
            {
                foreach (Transform child in t)
                {
                    getGeometryIter(child);
                }
            }
        }


        public bool HasHiddenGeo
        {
            get
            {
                if (geometryPassiveList.Count < 1 || geometryPassiveList[0].activeSelf)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }


    }
}
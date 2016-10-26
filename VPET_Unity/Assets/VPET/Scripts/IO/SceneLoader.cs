/*
-----------------------------------------------------------------------------
This source file is part of VPET - Virtual Production Editing Tool
http://vpet.research.animationsinstitut.de/
http://github.com/FilmakademieRnd/VPET

Copyright (c) 2016 Filmakademie Baden-Wuerttemberg, Institute of Animation

This project has been realized in the scope of the EU funded project Dreamspace
under grant agreement no 610005.
http://dreamspaceproject.eu/

This program is free software; you can redistribute it and/or modify it under
the terms of the GNU Lesser General Public License as published by the Free Software
Foundation; version 2.1 of the License.

This program is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License along with
this program; if not, write to the Free Software Foundation, Inc., 59 Temple
Place - Suite 330, Boston, MA 02111-1307, USA, or go to
http://www.gnu.org/licenses/old-licenses/lgpl-2.1.html
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
	public enum LightTypeKatana { disk, directional, sphere, rect }

	public class SceneLoader : MonoBehaviour 
	{
	    //!
	    //! name of the parent gameobject, all objects go underneath it
	    //!
	    public string sceneParent = "Scene";
	
	
	    private GameObject scnPrtGO;
	    private List<Texture2D> sceneTextureList = new List<Texture2D>();
	    private List<Mesh[]> sceneMeshList = new List<Mesh[]>();
	    private List<GameObject> sceneEditableObjects = new List<GameObject>();
	    private GameObject scnRoot;
	
	    private List<GameObject> sceneCameraList = new List<GameObject>();
	    public List<GameObject> SceneCameraList
	    {
	        get { return sceneCameraList;  }
	    }

        private List<GameObject> geometryPassiveList = new List<GameObject>();

        void Start()
	    {
	        // create scene parent if not there
	        scnPrtGO = GameObject.Find( sceneParent );
	        if ( scnPrtGO == null )
	        {
	            scnPrtGO = new GameObject( sceneParent );
	        }
	
	        scnRoot = scnPrtGO.transform.FindChild("root").gameObject;
	        if (scnRoot == null)
	        {
	            scnRoot = new GameObject("root");
	            scnRoot.transform.parent = scnPrtGO.transform;
	        }
	
	    }	

        public void ResetScene()
        {
            sceneEditableObjects.Clear();
            sceneTextureList.Clear();
            sceneMeshList.Clear();
            sceneCameraList.Clear();
            geometryPassiveList.Clear();


            if (scnRoot != null)
            {
                foreach (Transform child in scnRoot.transform)
                {
                    GameObject.Destroy(child.gameObject);
                }
            }            
        }

        public void createSceneGraph( SceneObjectKatana scnObjKtn  )
	    {
	        // create textures
	        if (VPETSettings.Instance.doLoadTextures )
	        {
	            createTextures(scnObjKtn);
	        }      
	        scnObjKtn.rawTextureList.Clear();
	
	        // create meshes
	        createMeshes(scnObjKtn);
	        scnObjKtn.rawVertexList.Clear();
	        scnObjKtn.rawIndexList.Clear();
	        scnObjKtn.rawNormalList.Clear();
	        scnObjKtn.rawUvList.Clear();
	
	        // iterate nodes
	        createSceneGraphIter(scnObjKtn, scnRoot.transform, 0);
	        scnObjKtn.rawNodeList.Clear();
	
	        // make editable        
	        foreach ( GameObject g in sceneEditableObjects )
	        {
	            SceneObject sobj = g.GetComponent<SceneObject>();
	            if ( sobj == null )
	            {
	                g.AddComponent<SceneObject>();
	            }
	        }
	
	
	        /*
	        Transform geoTransform = GameObject.Find( "geo" ).transform;
	        if ( geoTransform != null )
	        {
	            print( "Set geo transform" );
	            JoystickInput joystickScript = GameObject.Find( "JoystickInput" ).gameObject.GetComponent<JoystickInput>();
	            joystickScript.WorldTransform = geoTransform;        
	        }
	         */
	
	
	
	    }
	
	
	    private int createSceneGraphIter( SceneObjectKatana scnObjKtn, Transform parent, int idx  )
	    {
	        GameObject obj = null; // = new GameObject( scnObjKtn.rawNodeList[idx].name );
	        
	        Node node = scnObjKtn.rawNodeList[idx];
	
	        
	        if ( node.GetType() == typeof( NodeGeo ) )
	        {
	            NodeGeo nodeGeo = (NodeGeo)Convert.ChangeType( node, typeof( NodeGeo ) );
	            obj = createObject( nodeGeo, parent );
	        }
	        else if ( node.GetType() == typeof( NodeLight ) )
	        {
	            NodeLight nodeLight = (NodeLight)Convert.ChangeType( node, typeof( NodeLight ) );
	            // HACK: remove and support skydomes
	            if (nodeLight.name.ToLower() == "skydome")
	            {
	                print("Do Support SkyDome !");
	            }
	            else
	            {
	                obj = createLight(nodeLight, parent);
	            }
	        }
	        else if ( node.GetType() == typeof( NodeCam ) )
	        {
	            NodeCam nodeCam = (NodeCam)Convert.ChangeType( node, typeof( NodeCam ) );
	            obj = createCamera( nodeCam, parent );
	        }
	        else
	        {
	            obj = createNode( node, parent );
	        }
	
	        if ( node.editable )
	        {
	            sceneEditableObjects.Add( obj );
	        }
	
	
	
	        int idxChild = idx;
	
	        for ( int k = 1; k <= node.childCount; k++ )
	        {
	            idxChild = createSceneGraphIter( scnObjKtn, obj.transform, idxChild+1 );
	        }
	
	
	
	        return idxChild;
	    }
	
	
	    private void createTextures( SceneObjectKatana scnObjKtn )
	    {
	
	
	        foreach ( byte[] tex in scnObjKtn.rawTextureList )
	        {
	            //print( "text byte size: " + tex.Length );
	            //GameObject obj = GameObject.CreatePrimitive( PrimitiveType.Plane );
	            //Material mat = new Material( Shader.Find( "Standard" ) );
	            Texture2D tex_2d = new Texture2D( 16, 16, TextureFormat.DXT5Crunched, false );
	            tex_2d.LoadImage( tex );
	            //mat.SetTexture( "_MainTex", tex_2d );
	            //obj.GetComponent<Renderer>().material = mat;
	            /*
	            if ( tex_2d.width > 1000 || tex_2d.height > 1000)
	            {
	                tex_2d.Resize( 64, 64, TextureFormat.RGB24, false );
	                //tex_2d.Apply();
	            }
	             */
	            sceneTextureList.Add( tex_2d );
	        }
	    }
	
	
	    //!
		//! function ??
	    //! @param  ??   ??
	    //!    
	    private void createMeshes( SceneObjectKatana scnObjKtn )
	    {
	        for ( int k = 0; k<scnObjKtn.rawVertexList.Count; k++ )
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
	            createSplitMesh( verts, norms, uvs, tris );
	
	        }
	
	        /*
	        for( int i=0; i<sceneMeshList.Count; i++ )
	        {
	            foreach( Mesh m in sceneMeshList[i] )
	            {
	                GameObject obj = new GameObject();
	                obj.AddComponent<MeshFilter>();
	                obj.GetComponent<MeshFilter>().mesh = m;
	                obj.AddComponent<MeshRenderer>();
	            }
	        }
	        */
	
	    }
	
	
	
	    //!
	    //! function create the object from mesh data
	    //! @param  scnObjKtn   object which holds the data
	    //!
	    private GameObject createNode( Node node, Transform parentTransform )
	    {
	        // Tranform
	        Vector3 pos = new Vector3( -node.position[0], node.position[1], node.position[2] );
	        //print( "Position: " + pos );
	        Quaternion rot = new Quaternion( node.rotation[0], -node.rotation[1], -node.rotation[2], -node.rotation[3] );
	        // Vector3 euler = rot.eulerAngles;
	        //print( "Euler: " + euler );
	        //rot = new Quaternion();
	        //Vector3 axis = new Vector3(0, 180, 0);
	        //rot.eulerAngles = euler+axis;
	        Vector3 scl = new Vector3( node.scale[0], node.scale[1], node.scale[2] );
	        //print( "Scale: " + scl );
	
	
	        // set up object basics
	        GameObject objMain = new GameObject();
	        objMain.name = node.name;
	
	        //place object
	        objMain.transform.parent = parentTransform; // GameObject.Find( "Scene" ).transform;
	        objMain.transform.localPosition =  pos; // new Vector3( 0, 0, 0 );
	        objMain.transform.localRotation =   rot; //  Quaternion.identity;
	        objMain.transform.localScale =    scl; // new Vector3( 1, 1, 1 );
	        objMain.layer = 0;
	
	
	
	
	        return objMain;
	    }

        //!
        //! function that determines if a texture has alpha
        //! @param  texture   the texture to be checked
        //!
        private bool hasAlpha(Texture2D texture)
        {
            TextureFormat textureFormat = texture.format;
            return (textureFormat == TextureFormat.Alpha8 ||
                textureFormat == TextureFormat.ARGB4444 ||
                textureFormat == TextureFormat.ARGB32 ||
                textureFormat == TextureFormat.DXT5 ||
                textureFormat == TextureFormat.PVRTC_RGBA2 ||
                textureFormat == TextureFormat.PVRTC_RGBA4 ||
                textureFormat == TextureFormat.ATC_RGBA8);
        }
	
	    //!
	    //! function create the object from mesh data
	    //! @param  scnObjKtn   object which holds the data
	    //!
	    private GameObject createObject( NodeGeo nodeGeo, Transform parentTransform )
	    {
	
	        // Material
	        Material mat = new Material( Shader.Find( "Standard" ) );
	        //available parameters in this physically based shader:
	        // _Color                   diffuse color (color including alpha)
	        // _MainTex                 diffuse texture (2D texture)
	        // _Cutoff                  alpha cutoff
	        // _Glossiness              smoothness of surface
	        // _Metallic                matallic look of the material
	        // _MetallicGlossMap        metallic texture (2D texture)
	        // _BumpScale               scale of the bump map (float)
	        // _BumpMap                 bumpmap (2D texture)
	        // _Parallax                scale of height map
	        // _ParallaxMap             height map (2D texture)
	        // _OcclusionStrength       scale of occlusion
	        // _OcclusionMap            occlusionMap (2D texture)
	        // _EmissionColor           color of emission (color without alpha)
	        // _EmissionMap             emission strength map (2D texture)
	        // _DetailMask              detail mask (2D texture)
	        // _DetailAlbedoMap         detail diffuse texture (2D texture)
	        // _DetailNormalMapScale    scale of detail normal map (float)
	        // _DetailAlbedoMap         detail normal map (2D texture)
	        // _UVSec                   UV Set for secondary textures (float)
	        // _Mode                    rendering mode (float) 0 -> Opaque , 1 -> Cutout , 2 -> Transparent
	        // _SrcBlend                source blend mode (enum is UnityEngine.Rendering.BlendMode)
	        // _DstBlend                destination blend mode (enum is UnityEngine.Rendering.BlendMode)
	        // test texture
	        // WWW www = new WWW("file://F:/XML3D_Examples/tex/casual08a.jpg");
	        // Texture2D texture = www.texture;
	        // meshRenderer.material.SetTexture("_MainTex",texture);
	
	        // Material Properties
	        mat.color = new Color( nodeGeo.color[0], nodeGeo.color[1], nodeGeo.color[2] );
	        mat.SetFloat( "_Glossiness", nodeGeo.roughness );
	
	        // Texture
	        if (nodeGeo.textureId > -1 && nodeGeo.textureId < sceneTextureList.Count)
	        {
                Texture2D texRef = sceneTextureList[nodeGeo.textureId];

                mat.SetTexture("_MainTex", texRef);

                // set materials render mode to fate to senable alpha blending
                if (hasAlpha(texRef))
                {
                    mat.SetFloat("_Mode", 2);
                    mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    mat.SetInt("_ZWrite", 0);
                    mat.DisableKeyword("_ALPHATEST_ON");
                    mat.EnableKeyword("_ALPHABLEND_ON");
                    mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    mat.renderQueue = 3000;
                }
            }

            // Tranform / convert handiness
            Vector3 pos = new Vector3( -nodeGeo.position[0], nodeGeo.position[1], nodeGeo.position[2] );
	        //print( "Position: " + pos );
	        // Rotation / convert handiness
	        Quaternion rot = new Quaternion( -nodeGeo.rotation[0], nodeGeo.rotation[1], nodeGeo.rotation[2], nodeGeo.rotation[3] );
	        //print("rot: " + rot.ToString());
	
	        //Quaternion rotTest = Quaternion.Euler(60f, -45f, -20f);
	        //print("rotTest: " + rotTest.ToString());
	        //rot = rotTest;
	
	        //Vector3 euler = rot.eulerAngles;
	        //print( "Euler ("+nodeGeo.name+"): " + euler );
	
	        //euler = new Vector3(euler.z-180f, euler.x, -euler.y);
	        //print("Euler (" + nodeGeo.name + "): " + euler);
	
	        // Quaternion rotY_180 = Quaternion.AngleAxis(180.0f, Vector3.up);
	
	
	        // rot = Quaternion.Euler(euler.x, euler.y, euler.z); // * rotY_180;
	
	        //euler = rot.eulerAngles;
	        //print("Euler (" + nodeGeo.name + "): " + euler);
	
	        // Scale
	        Vector3 scl = new Vector3( nodeGeo.scale[0], nodeGeo.scale[1], nodeGeo.scale[2] );
	        //print( "Scale: " + scl );
	
	
	        // set up object basics
	        GameObject objMain = new GameObject();
	        objMain.name = nodeGeo.name;
	
	        // Add Material
	        MeshRenderer meshRenderer = objMain.AddComponent<MeshRenderer>();
	        meshRenderer.material = mat;
	
	        // print(objMain.name + " and " + nodeGeo.geoId);
	
	
	        // Add Mesh
	        if ( nodeGeo.geoId > -1 && nodeGeo.geoId < sceneMeshList.Count )
	        {
	            Mesh[] meshes = sceneMeshList[nodeGeo.geoId];
	            objMain.AddComponent<MeshFilter>();
	            objMain.GetComponent<MeshFilter>().mesh  = meshes[0];
	            for( int i=1; i<meshes.Length; i++ )
	            {
	                GameObject subObj = new GameObject( objMain.name+"_part"+i.ToString() );
	                subObj.AddComponent<MeshFilter>();
	                subObj.GetComponent<MeshFilter>().mesh = meshes[i];
	                MeshRenderer subMeshRenderer = subObj.AddComponent<MeshRenderer>();
	                subMeshRenderer.material = mat;
	                subObj.transform.parent = objMain.transform;
	            }
	
	        }
	
	        //place object
	        objMain.transform.parent = parentTransform; // GameObject.Find( "Scene" ).transform;
	        objMain.transform.localPosition =  pos; // new Vector3( 0, 0, 0 );
	        objMain.transform.localRotation =   rot; //  Quaternion.identity;
	        objMain.transform.localScale =    scl; // new Vector3( 1, 1, 1 );
	        //objMain.layer = 0;
	

	        return objMain;
	
	    }
	
	
	    //!
	    //! function create the object from mesh data
	    //! @param  node   object which holds the data
	    //! @param  parentTransform   parent object
	    //!
	    private GameObject createLight( NodeLight nodeLight, Transform parentTransform )
	    {	
	
	        // Tranform
	        Vector3 pos = new Vector3( -nodeLight.position[0], nodeLight.position[1], nodeLight.position[2] );
	        Quaternion rot = new Quaternion( -nodeLight.rotation[0], nodeLight.rotation[1], nodeLight.rotation[2], nodeLight.rotation[3] );
	        Vector3 scl = new Vector3( nodeLight.scale[0], nodeLight.scale[1], nodeLight.scale[2] );
	
	        // set up object basics
	        GameObject objMain = new GameObject();
	        objMain.name = nodeLight.name;
	
	        //place object
			objMain.transform.SetParent(parentTransform, false );
	        objMain.transform.localPosition = pos; 
	        objMain.transform.localRotation = rot;
	        objMain.transform.localScale = scl; 
	
	        // Add light prefab
	        GameObject lightUber = Resources.Load<GameObject>("VPET/Prefabs/UberLight");
	        GameObject _lightUberInstance = Instantiate(lightUber);
	        _lightUberInstance.name = lightUber.name;
	
	        Light lightComponent = _lightUberInstance.GetComponent<Light>();
	        lightComponent.type = nodeLight.lightType;
	        lightComponent.color = new Color(nodeLight.color[0], nodeLight.color[1], nodeLight.color[2]);            
            lightComponent.intensity = nodeLight.intensity / VPETSettings.Instance.lightIntensityFactor;
            
			print("Create Light: " + nodeLight.name + " of type: " + ((LightTypeKatana)(nodeLight.lightType)).ToString() + " Intensity: " + nodeLight.intensity + " Pos: " + pos  );

            // Add light specific settings
            if (nodeLight.lightType == LightType.Directional)
	        {
	        }
	        else if (nodeLight.lightType == LightType.Spot)
	        {
	            lightComponent.spotAngle = Mathf.Min(150, nodeLight.angle);
	            lightComponent.range = 200;
	        }
	        else if (nodeLight.lightType == LightType.Area)
	        {
	            lightComponent.spotAngle = Mathf.Min(150, nodeLight.angle);
	            lightComponent.range = 200;
	        }
	        else
	        {
	        }
	
	
	        // parent 
	        _lightUberInstance.transform.SetParent(objMain.transform, false);
	
	        // add scene object for interactivity at the light quad
			SceneObject sco = objMain.AddComponent<SceneObject>();
			sco.exposure = nodeLight.exposure;
	
	        // TODO: what for ??
	        objMain.layer = 0;
	
	        return objMain;
	
	    }
	
	    //!
	    //! function create the object from mesh data
	    //! @param  node   object which holds the data
	    //! @param  parentTransform   parent object
	    //!
	    private GameObject createCamera( NodeCam nodeCam, Transform parentTransform )
	    {
	        // Tranform
	        Vector3 pos = new Vector3( -nodeCam.position[0], nodeCam.position[1], nodeCam.position[2] );
	        Quaternion rot = new Quaternion( -nodeCam.rotation[0], nodeCam.rotation[1], nodeCam.rotation[2], nodeCam.rotation[3] );
	        Vector3 scl = new Vector3( nodeCam.scale[0], nodeCam.scale[1], nodeCam.scale[2] );
	
	        // set up object basics
	        GameObject objMain = new GameObject();
	        objMain.name = nodeCam.name;
	
	        // add camera data script and set values
	        CameraObject camScript = objMain.AddComponent<CameraObject>();
	        camScript.fov = nodeCam.fov;
	        camScript.near = nodeCam.near;
	        camScript.far = nodeCam.far;
	
	        // place camera
	        objMain.transform.parent = parentTransform; 
	        objMain.transform.localPosition =  pos; 
	        objMain.transform.localRotation =   rot; 
	        objMain.transform.localScale =    scl; 
	        // Rotate 180 around y-axis because lights and cameras have additional eye space coordinate system
	        objMain.transform.Rotate(new Vector3(0, 180f, 0), Space.Self);
	
	        // TODO: what for ??
	        objMain.layer = 0;
	
	        // add to list for later access as camera location
	        sceneCameraList.Add( objMain );
	
	        return objMain;
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
	        // if more than 64K vertices, split the mesh in submeshs
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
	
	        sceneMeshList.Add( meshList.ToArray() );
	
	    }
	
	
	    //! function create mesh at the given gameobject and split if necessary
	    //! @param  obj             gameobject to work on
	    //! @param  vertices        
	    //! @param  normals        
	    //! @param  uvs        
	    //! @param  triangles        
	    //! @param  material        
	    private void createSplitObject( GameObject obj, Vector3[] vertices, Vector3[] normals, Vector2[] uvs, int[] triangles, Material material )
	    {
	        // TODO: review
	        // if more than 64K vertices, split the mesh in submeshs
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
	                    
	                GameObject subObj = new GameObject( obj.name+"_part"+subObjCount.ToString() );
	
	                // print( "create :" + subObj.name + " triIndex " + triIndex  + " triOffset " + triIndexOffset + " numVertices " + subVertices.Count);
	
	                subObj.AddComponent<MeshFilter>();
	                Mesh mesh = subObj.GetComponent<MeshFilter>().mesh;
	                mesh.Clear();
	                mesh.vertices = subVertices.ToArray();                  
	                mesh.normals = subNormals.ToArray();                    
	                mesh.uv = subUVs.ToArray();
	                mesh.triangles = subTriangles.ToArray();
	                MeshRenderer meshRenderer = subObj.AddComponent<MeshRenderer>();
	                meshRenderer.material = material;
	                subObj.transform.parent = obj.transform;
	
	                triIndex += triIndexOffset;
	
	                if ( triIndex+triIndexOffset > triIndexMax )
	                {
	                    triIndexOffset = triIndexMax-triIndex+1;
	                }
	
	            }
	        }
	        else
	        {
	            obj.AddComponent<MeshFilter>();
	            Mesh mesh = obj.GetComponent<MeshFilter>().mesh;
	            mesh.Clear();
	            mesh.vertices = vertices;
	            mesh.normals = normals;           
	            mesh.uv = uvs;
	            mesh.triangles = triangles;
	            MeshRenderer meshRenderer = obj.AddComponent<MeshRenderer>();
	            meshRenderer.material = material;
	            // mesh.RecalculateNormals();
	            // mesh.RecalculateBounds();
	        }
	
	    }

        public void HideGeometry()
        {
            if (geometryPassiveList.Count == 0 && scnRoot.transform.childCount > 0)
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
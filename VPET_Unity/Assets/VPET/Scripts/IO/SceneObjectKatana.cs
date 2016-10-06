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
﻿using UnityEngine;
using System.Collections;
using System;
using System.Text;
using System.IO;
using System.Collections.Generic;
// using System.Runtime.InteropServices;

public enum NodeType { GROUP, GEO, LIGHT, CAMERA }

/*
namespace vpet
{
    public enum LightType
    {
        Spot = 0,
        Directional = 1,
        Point = 2,
        Area = 3,
    }
}
 */

public class Node
{
    public NodeType type;
    public string name;
    public float[] position = new float[3];
    public float[] rotation = new float[4];
    public float[] scale = new float[3];
    public int childCount;
    public bool editable;
    protected byte[] sliceInt = new byte[size_int];
    protected byte[] sliceValues;
    protected int numValues = 0;

    protected const int size_int = sizeof( int );
    protected const int size_float = sizeof( float );


    public virtual void Parse( ref byte[] data, ref int dataIdx )
    {

        Array.Copy( data, dataIdx, sliceInt, 0, size_int );
        SceneObjectKatana.checkEndian( ref sliceInt );
        numValues = BitConverter.ToInt32( sliceInt, 0 );
        dataIdx += size_int;

        sliceValues = new byte[numValues];
        Array.Copy( data, dataIdx, sliceValues, 0, numValues );
        name =  ASCIIEncoding.ASCII.GetString( data, dataIdx, numValues );
        dataIdx += numValues;

        sliceValues = new byte[3*size_float];
        Array.Copy( data, dataIdx, sliceValues, 0, 3*size_float );
        SceneObjectKatana.checkEndian( ref sliceValues );
        for ( int i = 0; i < 3; i++ )
        {
            position[i] = BitConverter.ToSingle( sliceValues, i*size_float );
        }
        dataIdx += 3*size_float;

        sliceValues = new byte[4*size_float];
        Array.Copy( data, dataIdx, sliceValues, 0, 4*size_float );
        SceneObjectKatana.checkEndian( ref sliceValues );
        for ( int i = 0; i < 4; i++ )
        {
            rotation[i] = BitConverter.ToSingle( sliceValues, i*size_float );
        }
        dataIdx += 4*size_float;

        // Debug.Log( name + ": " + rotation[0] + " " + rotation[1] + " "  + rotation[2] + " "  + rotation[3] );

        sliceValues = new byte[3*size_float];
        Array.Copy( data, dataIdx, sliceValues, 0, 3*size_float );
        SceneObjectKatana.checkEndian( ref sliceValues );
        for ( int i = 0; i < 3; i++ )
        {
            scale[i] = BitConverter.ToSingle( sliceValues, i*size_float );
        }
        dataIdx += 3*size_float;

        sliceValues = new byte[size_int];
        Array.Copy( data, dataIdx, sliceValues, 0, size_int );
        childCount = BitConverter.ToInt32( sliceValues, 0 );
        dataIdx +=size_int;

        sliceValues = new byte[sizeof( bool )];
        Array.Copy( data, dataIdx, sliceValues, 0, sizeof( bool ) );
        editable = BitConverter.ToBoolean( sliceValues, 0 );
        dataIdx +=sizeof( bool );

    }
}

public class NodeGeo: Node
{
    public int geoId;
    public int textureId;
    public float[] color = new float[3];
    public float roughness;

    public override void Parse( ref byte[] data, ref int dataIdx )
    {

        base.Parse( ref data, ref dataIdx  );

        sliceValues = new byte[size_int];
        Array.Copy( data, dataIdx, sliceValues, 0, size_int );
        SceneObjectKatana.checkEndian( ref sliceValues );
        geoId = BitConverter.ToInt32( sliceValues, 0 );
        dataIdx += size_int;

        sliceValues = new byte[size_int];
        Array.Copy( data, dataIdx, sliceValues, 0, size_int );
        SceneObjectKatana.checkEndian( ref sliceValues );
        textureId = BitConverter.ToInt32( sliceValues, 0 );
        dataIdx += size_int;

        
        sliceValues = new byte[3*size_float];
        Array.Copy( data, dataIdx, sliceValues, 0, 3*size_float );
        SceneObjectKatana.checkEndian( ref sliceValues );
        for ( int i = 0; i < 3; i++ )
        {
            color[i] = BitConverter.ToSingle( sliceValues, i*size_float );
        }
        dataIdx += 3*size_float;

        sliceValues = new byte[size_float];
        Array.Copy( data, dataIdx, sliceValues, 0, size_float );
        SceneObjectKatana.checkEndian( ref sliceValues );
        roughness = BitConverter.ToSingle( sliceValues, 0 );
        dataIdx += size_float;

    }
};

class NodeLight : Node
{
    public LightType lightType;
    public float[] color = new float[3];
    public float intensity;
    public float angle;
	public float exposure = 3f;
    public override void Parse( ref byte[] data, ref int dataIdx )
    {
        base.Parse( ref data, ref dataIdx  );

        sliceValues = new byte[size_int];
        Array.Copy( data, dataIdx, sliceValues, 0, size_int );
        SceneObjectKatana.checkEndian( ref sliceValues );
        lightType = (LightType)BitConverter.ToInt32( sliceValues, 0 );
        dataIdx += size_int;


        /*
        char value = Convert.ToChar( data[dataIdx] );
        lightType = (LightType)int.Parse( value.ToString() );
        dataIdx += 1;
        */


        sliceValues = new byte[3*size_float];
        Array.Copy( data, dataIdx, sliceValues, 0, 3*size_float );
        SceneObjectKatana.checkEndian( ref sliceValues );
        for ( int i = 0; i < 3; i++ )
        {
            color[i] = BitConverter.ToSingle( sliceValues, i*size_float );
        }
        dataIdx += 3*size_float;

        sliceValues = new byte[size_float];
        Array.Copy( data, dataIdx, sliceValues, 0, size_float );
        SceneObjectKatana.checkEndian( ref sliceValues );
        intensity = BitConverter.ToSingle( sliceValues, 0 );
        dataIdx += size_float;

        sliceValues = new byte[size_float];
        Array.Copy( data, dataIdx, sliceValues, 0, size_float );
        SceneObjectKatana.checkEndian( ref sliceValues );
        angle = BitConverter.ToSingle( sliceValues, 0 );
        dataIdx += size_float;

		sliceValues = new byte[size_float];
		Array.Copy( data, dataIdx, sliceValues, 0, size_float );
		SceneObjectKatana.checkEndian( ref sliceValues );
		exposure = BitConverter.ToSingle( sliceValues, 0 );
		dataIdx += size_float;
	}
};

class NodeCam : Node
{
    public float fov;
    public float near;
    public float far;
    public override void Parse( ref byte[] data, ref int dataIdx )
    {
        base.Parse( ref data, ref dataIdx );

        sliceValues = new byte[size_float];
        Array.Copy( data, dataIdx, sliceValues, 0, size_float );
        SceneObjectKatana.checkEndian( ref sliceValues );
        fov = BitConverter.ToSingle( sliceValues, 0 );
        dataIdx += size_float;

        sliceValues = new byte[size_float];
        Array.Copy( data, dataIdx, sliceValues, 0, size_float );
        SceneObjectKatana.checkEndian( ref sliceValues );
        near = BitConverter.ToSingle( sliceValues, 0 );
        dataIdx += size_float;

        sliceValues = new byte[size_float];
        Array.Copy( data, dataIdx, sliceValues, 0, size_float );
        SceneObjectKatana.checkEndian( ref sliceValues );
        far = BitConverter.ToSingle( sliceValues, 0 );
        dataIdx += size_float;

    }
};


//!
//! Class to parse and hold the geometry data for a single mesh object
//!

public class SceneObjectKatana : System.Object
{
    //!
    //! The path to the object with object name as leaf
    //!
    //public string dagpath;
    //!
    //! Editable Object
    //!
    //public int rawEditable = 0;
    //!
    //! Mesh data: vertices
    //!
    //public float[] rawVertices;
    //!
    //! Mesh data: indices
    //!
    //public int[] rawIndices;
    //!
    //! Mesh data: normals
    //!
    //public float[] rawNormals;
    //!
    //! Mesh data: uvs
    //!
    //public float[] rawUvs;
    //!
    //! Transform data: position
    //!
    //public float[] rawPosition;
    //!
    //! Transform data: rotation
    //!
    // public float[] rawRotation;
    //!
    //! Transform data: scale
    //!
    //public float[] rawScale;
    //!
    //! Texture data: width
    //!
    //public int rawTexWidth = 64;
    //!
    //! Texture data: height
    //!
    //public int rawTexHeight = 64;
    //!
    //! Texture data: texture
    //!
    //public byte[] rawTexture;
    //!
    //! Material data: base color
    //!
    //public float[] rawColor;
    //!
    //! Material data: base color
    //!
    //public float rawRoughness;

    ~SceneObjectKatana()
    {
        rawTextureList.Clear();
        rawVertexList.Clear();
        rawIndexList.Clear();
        rawNormalList.Clear();
        rawUvList.Clear();
        rawNodeList.Clear();
    }


    public List<byte[]> rawTextureList = new List<byte[]>();

    public List<float[]> rawVertexList = new List<float[]>();
    public List<int[]> rawIndexList = new List<int[]>();
    public List<float[]> rawNormalList = new List<float[]>();
    public List<float[]> rawUvList = new List<float[]>();


    public List<Node> rawNodeList = new List<Node>();

    private const int size_int = sizeof( int );
    private const int size_float = sizeof( float );




    //!
    //! function to check and reverse the endian order ( assume message from server adapter is little endian )
    //!
    public static void checkEndian( ref byte[] dataNumber )
    {
        if ( !BitConverter.IsLittleEndian )
            Array.Reverse( dataNumber );
    }


    public bool parseNode( byte[] data )
    {
        int dataIdx = 0;
        byte[] sliceInt = new byte[size_int];

        while ( dataIdx < data.Length-1 )
        {

            Array.Copy( data, dataIdx, sliceInt, 0, size_int );
            checkEndian( ref sliceInt );
            NodeType nodeType = (NodeType)BitConverter.ToInt32( sliceInt, 0 );
            dataIdx += size_int;
            
            switch (nodeType)
            {
                case NodeType.GROUP:
                    Node node = new Node();
                    node.type = nodeType;
                    node.Parse( ref data, ref dataIdx );
                    rawNodeList.Add( node );
                    break;
                case NodeType.GEO:
                    NodeGeo nodeGeo = new NodeGeo();
                    nodeGeo.type = nodeType;
                    nodeGeo.Parse( ref data, ref dataIdx );
                    rawNodeList.Add( nodeGeo );
                    break;
                case NodeType.LIGHT:
                    NodeLight nodeLight = new NodeLight();
                    nodeLight.type = nodeType;
                    nodeLight.Parse( ref data, ref dataIdx );
                    rawNodeList.Add( nodeLight );
                    break;
                case NodeType.CAMERA:
                    NodeCam nodeCam = new NodeCam();
                    nodeCam.type = nodeType;
                    nodeCam.Parse( ref data, ref dataIdx );
                    rawNodeList.Add( nodeCam );
                    break;
            }

            // Debug.Log( "Process Node: " + rawNodeList[rawNodeList.Count-1].name ); 

        }

        return true;
    }

    public bool parseObject( byte[] data )
    {
        int dataIdx = 0;

        byte[] sliceInt = new byte[size_int];


        while ( dataIdx < data.Length-1 )
        {

            // get vertices
            Array.Copy( data, dataIdx, sliceInt, 0, size_int );
            checkEndian( ref sliceInt );
            int numValues = BitConverter.ToInt32( sliceInt, 0 );
            dataIdx += size_int;
            byte[] sliceValues = new byte[numValues*size_float];
            Array.Copy( data, dataIdx, sliceValues, 0, numValues*size_float );
            checkEndian( ref sliceValues );

            float[] rawFloatData = new float[numValues];

            for ( int i = 0; i < numValues; i++ )
            {
                rawFloatData[i] = BitConverter.ToSingle( sliceValues, i*size_float );
            }
            dataIdx += numValues*size_float;

            rawVertexList.Add( rawFloatData );

            //Debug.Log( "VertexCount " + rawFloatData.Length/3 + " at " + (rawVertexList.Count-1) );


            // get indices
            Array.Copy( data, dataIdx, sliceInt, 0, size_int );
            checkEndian( ref sliceInt );
            numValues = BitConverter.ToInt32( sliceInt, 0 );
            dataIdx += size_int;
            sliceValues = new byte[numValues*size_int];
            Array.Copy( data, dataIdx, sliceValues, 0, numValues*size_int );
            checkEndian( ref sliceValues );

            int[] rawIntData = new int[numValues];

            for ( int i = 0; i < numValues; i++ )
            {
                rawIntData[i] = BitConverter.ToInt32( sliceValues, i*size_int );
            }
            dataIdx += numValues*size_int;

            rawIndexList.Add( rawIntData );


            //Debug.Log( "IndexCount " + rawIntData.Length + " at " + (rawIndexList.Count-1) );



            // get normals
            Array.Copy( data, dataIdx, sliceInt, 0, size_int );
            checkEndian( ref sliceInt );
            numValues = BitConverter.ToInt32( sliceInt, 0 );
            dataIdx += size_int;
            sliceValues = new byte[numValues*size_float];
            Array.Copy( data, dataIdx, sliceValues, 0, numValues*size_float );
            checkEndian( ref sliceValues );

            rawFloatData = new float[numValues];

            for ( int i = 0; i < numValues; i++ )
            {
                rawFloatData[i] = BitConverter.ToSingle( sliceValues, i*size_float );
            }
            dataIdx += numValues*size_float;

            rawNormalList.Add( rawFloatData );

            // get uvs
            Array.Copy( data, dataIdx, sliceInt, 0, size_int );
            checkEndian( ref sliceInt );
            numValues = BitConverter.ToInt32( sliceInt, 0 );
            dataIdx += size_int;
            sliceValues = new byte[numValues*size_float];
            Array.Copy( data, dataIdx, sliceValues, 0, numValues*size_float );
            checkEndian( ref sliceValues );

            rawFloatData = new float[numValues];

            for ( int i = 0; i < numValues; i++ )
            {
                rawFloatData[i] = BitConverter.ToSingle( sliceValues, i*size_float );
            }
            dataIdx += numValues*size_float;

            rawUvList.Add( rawFloatData );

        }


        return true;
    }

    public bool parseTexture( byte[] data )
    {
        int dataIdx = 0;

        byte[] sliceInt = new byte[size_int];

        while ( dataIdx < data.Length-1 )
        {

            // get texture data 
            Array.Copy( data, dataIdx, sliceInt, 0, size_int );
            checkEndian( ref sliceInt );
            int numValues = BitConverter.ToInt32( sliceInt, 0 );
            dataIdx += size_int;

            byte[] rawData = new byte[numValues];
            Array.Copy( data, dataIdx, rawData, 0, numValues );
            checkEndian( ref rawData );
            dataIdx += numValues;

            rawTextureList.Add( rawData );


        }

        return true;

    }



}

/*
-----------------------------------------------------------------------------
This source file is part of VPET - Virtual Production Editing Tool
http://vpet.research.animationsinstitut.de/
http://github.com/FilmakademieRnd/v-p-e-t

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

#include "PluginState.h"
#include "GeometryScenegraphLocationDelegate.h"

#include <FnRenderOutputUtils/FnRenderOutputUtils.h>
#include <glm/gtx/string_cast.hpp>
#include <glm/gtx/matrix_decompose.hpp>
#include <glm/glm.hpp>

#include <string>
#include <fstream>


GeometryScenegraphLocationDelegate* GeometryScenegraphLocationDelegate::create()
{
    return new GeometryScenegraphLocationDelegate();
}

void GeometryScenegraphLocationDelegate::flush()
{
}

std::string GeometryScenegraphLocationDelegate::getSupportedRenderer() const
{
    return std::string("sceneDistributor");
}

void GeometryScenegraphLocationDelegate::fillSupportedLocationList(std::vector<std::string>& supportedLocationList) const
{
    supportedLocationList.push_back(std::string("polymesh"));
    supportedLocationList.push_back(std::string("subdmesh")); // ??
    // supportedLocationList.push_back(std::string("faceset"));
}

FnAttribute::Attribute GeometryScenegraphLocationDelegate::GetAttribute( FnAttribute::GroupAttribute i_attr, std::string i_name )
{
    for (int i = 0; i < i_attr.getNumberOfChildren(); ++i)
    {
        FnAttribute::Attribute childAttr = i_attr.getChildByIndex(i);
        std::string childName = i_attr.getChildName(i);

        if ( childName == i_name )
        {
            return childAttr;
        }
        else if ( childAttr.getType()  == kFnKatAttributeTypeGroup )
        {
            FnAttribute::Attribute result = GetAttribute( childAttr, i_name );
            if ( result.isValid() )
                return result;
        }
    }

    FnAttribute::Attribute tAttr = FnAttribute::Attribute();
    return tAttr;
}

bool GeometryScenegraphLocationDelegate::LoadMap( std::string i_filepath, unsigned char* &o_buffer,  int* o_bufferSize )
{
    std::ifstream infile;
    infile.open( i_filepath.c_str(), std::ios::binary | std::ios::in );
    if ( infile )
    {
        // get length of file:
        infile.seekg (0, infile.end);
        int length = infile.tellg();
        infile.seekg (0, infile.beg);

        char* buffer = new char [length];

        // read data as a block:
        infile.read (buffer, length);
        infile.close();

        o_buffer = (unsigned char*)buffer;
        *o_bufferSize = length;
        return true;
    }

    return false;
}


void* GeometryScenegraphLocationDelegate::process(FnKat::FnScenegraphIterator sgIterator, void* optionalInput)
{

    FnKat::FnScenegraphIterator sgMaterial = sgIterator;

    // Search for facesets
    for (FnKat::FnScenegraphIterator firstChild = sgIterator.getFirstChild(); firstChild.isValid(); firstChild = firstChild.getNextSibling())
    {
        if ( firstChild.getType() == "faceset" )
        {
            sgMaterial = firstChild;
            // TODO: Add a submesh and its material, currently this loop only returns last child for material-lookup
        }
    }

    // get state
    Dreamspace::Katana::SceneDistributorPluginState* sharedState = reinterpret_cast<Dreamspace::Katana::SceneDistributorPluginState*>(optionalInput);

    // create geo node
    Dreamspace::Katana::NodeGeo* nodeGeo =  new Dreamspace::Katana::NodeGeo();
    

	/*
	std::string dagpath = // dagpath to referecnce

	int i = -1;
	for (int i = 0; i < sharedState->objPackList.size(); ++i)
	{
		if (sharedState->objPackList[i].dagpath == dagpath)
		{
			break;
		}
	}

	if (i < texPackList.size())
	{
        nodeGeo->objId = i;
		return;
	}
	*/


    std::string instanceID = "";
    FnAttribute::GroupAttribute instanceGroup = sgIterator.getAttribute("instance");
    if ( instanceGroup.isValid() )
    {
        FnAttribute::StringAttribute instanceSource = sgIterator.getAttribute("instance.ID");
        instanceID = instanceSource.getValue();

        if ( instanceID != "" )
        {
            std::cout << "[INFO SceneDistributor.GeometryScenegraphLocationDelegate] instanceID : " << instanceID << std::endl;

            int i = 0;
            for (; i < sharedState->objPackList.size(); ++i )
            {
                if (sharedState->objPackList[i].instanceId == instanceID)
                {
                    break;
                }
            }

            if (i < sharedState->objPackList.size())
            {
                nodeGeo->geoId = i;
                std::cout << "[INFO SceneDistributor.GeometryScenegraphLocationDelegate] Instantiate to: " << nodeGeo->geoId << std::endl;
            }
        }
    }


    // Name
    // objPack->name = sgIterator.getFullName();
    // std::cout << "[INFO SceneDistributor.GeometryScenegraphLocationDelegate] Processing location: " << objPack->name << std::endl;


    if ( nodeGeo->geoId < 0 )
    {



        // Create Unity Package
        Dreamspace::Katana::ObjectPackage objPack;

        objPack.instanceId = instanceID;

        // Geometry data
        FnAttribute::IntAttribute startIndexAttr = sgIterator.getAttribute("geometry.poly.startIndex");
        FnAttribute::IntAttribute vertexListAttr = sgIterator.getAttribute("geometry.poly.vertexList");
        FnAttribute::FloatAttribute PAttr = sgIterator.getAttribute("geometry.point.P");
        FnAttribute::FloatAttribute NAttr = sgIterator.getAttribute("geometry.vertex.N");
        // FnAttribute::FloatAttribute UVAttr = sgIterator.getAttribute("geometry.vertex.UV");
        FnAttribute::IntAttribute indexAttr = sgIterator.getAttribute("geometry.arbitrary.st.index");
        FnAttribute::FloatAttribute indexedValueAttr = sgIterator.getAttribute("geometry.arbitrary.st.indexedValue");
        // FnAttribute::FloatAttribute valueAttr = sgIterator.getAttribute("geometry.arbitrary.st.value");
        // Transparency
        // FnAttribute::FloatAttribute maskAttr = sgIterator.getAttribute("material.prmanSurfaceParams.mask");
        // FnAttribute::StringAttribute maskMapAttr = sgIterator.getAttribute("material.prmanSurfaceParams.MaskMap");
        // Faces
        FnAttribute::IntConstVector vertexListData = vertexListAttr.getNearestSample(0.0f);
        FnAttribute::IntConstVector startIndexData = startIndexAttr.getNearestSample(0.0f);
        // Vertices / Points
        FnAttribute::FloatConstVector PData = PAttr.getNearestSample(0.0f);

        // Get vertices
        for (int i = 0; i < PData.size(); i++)
        {
            objPack.vertices.push_back(PData[i]);
        }

        // Normal
        FnAttribute::FloatConstVector NData;
        if (NAttr.isValid())
        {
            NData = NAttr.getNearestSample(0.0f);
        }

        // UV
        FnAttribute::FloatConstVector UVData;
        FnAttribute::IntConstVector uvIndexData;
        if ( indexedValueAttr.isValid() )
        {
            UVData = indexedValueAttr.getNearestSample(0.0f);
            uvIndexData = indexAttr.getNearestSample(0.0f);
        }


        // std::cout << "Prepare Geo" << std::endl;

        // Get indices, normals, uvs
        // Iter over polygon indices, convert n-gons to triangles and store indices pointing to the vertex data
        // rebuild normal- and uv-data arrays to get one normal/uv per vertex (using the same indices)
        // TODO: split vertex at splited uv map
        // Normal
        std::vector<glm::vec3> normals(PData.size()/3.0, glm::vec3(0.0) );

        // Uv
        std::vector<glm::vec2 > uvs( PData.size()/3.0, glm::vec2(0.0) );

        int numPolys = startIndexData.size() - 1;
        for (int poly = 0; poly < numPolys; poly++)
        {
            const int firstIndex = startIndexData[poly];
            const int lastIndex = startIndexData[poly + 1] - 1;

            for (int i = firstIndex+1; i < lastIndex; i++)
            {
                objPack.indices.push_back(vertexListData[firstIndex]);
                objPack.indices.push_back(vertexListData[i]);
                objPack.indices.push_back(vertexListData[i + 1]);

                if ( NAttr.isValid()) // get normals
                {
                    // normals for every vertex
                    // add values from face vertex normals and store per vertex id
                    normals[vertexListData[firstIndex]] += glm::vec3( NData[firstIndex*3], NData[firstIndex*3+1], NData[firstIndex*3+2] );
                    normals[vertexListData[i]] += glm::vec3( NData[i*3], NData[i*3+1], NData[i*3+2] );
                    normals[vertexListData[i+1]] += glm::vec3( NData[(i+1)*3], NData[(i+1)*3+1], NData[(i+1)*3+2] );
                }
                else // calculate normals
                {
                    // point indices
                    int pIdx1 = vertexListData[firstIndex]*3;
                    int pIdx2 = vertexListData[i]*3;
                    int pIdx3 = vertexListData[i+1]*3;

                    // face normal
                    glm::vec3 a = glm::vec3( PData[pIdx2], PData[pIdx2+1], PData[pIdx2+2] ) - glm::vec3( PData[pIdx1], PData[pIdx1+1], PData[pIdx1+2] );
                    glm::vec3 b = glm::vec3( PData[pIdx3], PData[pIdx3+1], PData[pIdx3+2] ) - glm::vec3( PData[pIdx2], PData[pIdx2+1], PData[pIdx2+2] );
                    glm::vec3 n = glm::cross( b, a );

                    normals[vertexListData[firstIndex]] += n;
                    normals[vertexListData[i]] += n;
                    normals[vertexListData[i+1]] += n;
                }

                if ( indexedValueAttr.isValid() ) // get uvs
                {
                    // same vor UVs but two values per index
                    // use different index map (st.index)
                    int uvIndex = uvIndexData[firstIndex];

                    uvs[vertexListData[firstIndex]] = glm::vec2( UVData[uvIndex*2], UVData[uvIndex*2+1] );

                    uvIndex = uvIndexData[i];
                    uvs[vertexListData[i]] = glm::vec2( UVData[uvIndex*2], UVData[uvIndex*2+1] );

                    uvIndex = uvIndexData[i+1];
                    uvs[vertexListData[i+1]] = glm::vec2( UVData[uvIndex*2], UVData[uvIndex*2+1] );
                }
            }
        }

        // fill normals float array
        for ( int i=0; i<normals.size(); i++ )
        {
            glm::vec3 n = glm::normalize(normals[i]);
            objPack.normals.push_back(n[0]);
            objPack.normals.push_back(n[1]);
            objPack.normals.push_back(n[2]);
        }

        // fill uvs float array
        for ( int i=0; i<uvs.size(); i++ )
        {
            glm::vec2 uv = uvs[i];
            objPack.uvs.push_back( uv[0] );
            objPack.uvs.push_back(uv[1]);
        }


        // store the object package
        sharedState->objPackList.push_back(objPack);

        // get geo id
        nodeGeo->geoId = sharedState->objPackList.size()-1;

    } // if ( nodeGeo->geoId < 0 )


    // std::cout << "VertexCount " << objPack.vertices.size()/3 << " at " << sharedState->objPackList.size() << std::endl;
    // std::cout << "IndexCount " << objPack.indices.size() << " at " << sharedState->objPackList.size() << std::endl;
    // std::cout << "Prepare Material" << std::endl;


    // Material
    FnAttribute::GroupAttribute materialAttr = FnKat::RenderOutputUtils::getFlattenedMaterialAttr(sgMaterial, sharedState->materialTerminalNamesAttr);
    // Retrieve only changed and overridden attributes (i.e. no default values)
    FnAttribute::GroupAttribute groupAttr = materialAttr.getChildByName("parameters");

    // Set color to white
    nodeGeo->color[0] = nodeGeo->color[1] = nodeGeo->color[2] = 1.0;
    nodeGeo->roughness = 0.111;

    if ( groupAttr.isValid() )
    {
        // std::cout << "childcount: " << tmpAttr.getNumberOfChildren() << std::endl;

        // get diffuse color
        FnAttribute::FloatAttribute colorAttr = groupAttr.getChildByName( "Kd" );
        if ( !colorAttr.isValid() )
        {
            // try to get the transmitance color instead
            colorAttr = groupAttr.getChildByName( "Kt" );
        }

        if ( colorAttr.isValid() )
        {
            // Get the color value
            FnAttribute::FloatConstVector colorData = colorAttr.getNearestSample(0.0f);

            nodeGeo->color[0] = colorData[0];
            nodeGeo->color[1] = colorData[1];
            nodeGeo->color[2] = colorData[2];
        }
        else
        {
            // Set color to white
            nodeGeo->color[0] = nodeGeo->color[1] = nodeGeo->color[2] = 1.0;
        }

        // get roughness
        FnAttribute::FloatAttribute roughnessAttr = groupAttr.getChildByName( "roughness" );
        if ( !roughnessAttr.isValid() )
        {
            roughnessAttr = groupAttr.getChildByName( "sigma" );
        }

        float value = roughnessAttr.getValue( 0.33, false );
        nodeGeo->roughness = value;
    }

    FnAttribute::StringAttribute fileAttr = sgIterator.getAttribute("textures.Kd_filename");

    if ( !fileAttr.isValid() )
        fileAttr = groupAttr.getChildByName( "Kd_filename" );


    //std::cout << "Prepare Texture" << std::endl;


    if ( fileAttr.isValid() )
    {
		std::string filePath = fileAttr.getValue();

		// check extension for "png"
		int posDel = filePath.rfind("."); // TODO: dot may be part of the relative path, e.g.: ./textures/map_no_ext
		if(posDel == std::string::npos) // no extension
		{
		}
		else if(filePath.substr(posDel+1) != "jpg") // TODO: ignore case
		{
			// std::string extPng = "png";
			std::string extPng = "jpg";
			filePath = filePath.substr(0, posDel+1) + extPng;
		}

		std::cout << "Map: "<< filePath << std::endl;

		Dreamspace::Katana::TexturePackage texPack;
		texPack.path = filePath;

        int i = 0;
        for (; i < sharedState->texPackList.size(); ++i )
		{
			if (sharedState->texPackList[i].path == filePath)
			{
                // std::cout << "filePath" << filePath << " at " << i << std::endl;
				break;
			}
		}

        if (i < sharedState->texPackList.size())
		{
            nodeGeo->textureId = i;
		}
		else
		{
			// try load the image
            if (!LoadMap(filePath, texPack.colorMapData, &texPack.colorMapDataSize))
			{
				std::cout << "Error reading map" << std::endl;
			}
			else
			{
                std::cout << "Done reading map" << std::endl;
                sharedState->texPackList.push_back(texPack);
                nodeGeo->textureId = sharedState->texPackList.size() - 1;
			}
		}
    }
    else
    {
        std::cout << "FILE ATTRIBUTE NOT VALID !!!" << std::endl;
    }


    // store at sharedState to access it in iterator
	sharedState->node = nodeGeo;

    sharedState->numObjectNodes++;

    /*

    // Debug
    std::string needle = "Urn_03Shape";
    if ( objPack->name.find( needle ) != std::string::npos )
    {

        std::cout << "Debug Urn 03" << std::endl;
        std::cout << objPack->name << std::endl;
        std::cout << "vertexListData count:" << vertexListData.size() << std::endl;
        std::cout << "NData count " << NData.size() << std::endl;
        std::cout << "UVData count " << UVData.size() << std::endl;
        std::cout << "Index count:" << objPack->iSize << std::endl;
        std::cout << "Normal Size:" << objPack->nSize << std::endl;
        std::cout << "Vertex Size:" << objPack->vSize << std::endl;
        std::cout << "Uv Size:" << objPack->uSize << std::endl;
        std::cout << "Debug Urn Glas" << std::endl;
        std::cout << "ST Indices count " << uvIndexData.size() << std::endl;
        std::cout << "Roughness " << objPack->roughness << std::endl;
        std::cout << "tex buffer size: " << objPack->colorMapDataSize << std::endl;

        if ( materialAttr.isValid() )
        {
            std::cout << "[INFO SceneDistributor.GeometryScenegraphLocationDelegate] Material:" << std::endl;
            std::cout << materialAttr.getXML() << std::endl;
        }
        else
        {
            std::cout << "No valid Material" << std::endl;
        }
    }
    */

    return NULL;

}

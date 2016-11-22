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
the terms of the MIT License as published by the Open Source Initiative.

This program is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE. See the MIT License for more details.

You should have received a copy of the MIT License along with
this program; if not go to
https://opensource.org/licenses/MIT
-----------------------------------------------------------------------------
*/
#ifndef COMPANYNAMEKATANA_PLUGINSTATE_H
#define COMPANYNAMEKATANA_PLUGINSTATE_H

#include <FnAttribute/FnAttribute.h>
//#include <FnBufferManager/BufferManagerDefinitions.h>

#include <stack>

#include <glm/mat4x4.hpp>
#include <glm/vec3.hpp>
#include <glm/gtc/quaternion.hpp>


namespace Dreamspace
{
namespace Katana
{
    enum LodMode { ALL, TAG };

    enum NodeType { GROUP, GEO, LIGHT, CAMERA };

    enum LightType { SPOT, DIRECTIONAL, POINT, AREA, NONE };

    /*
    struct MaterialPackage
    {
        float* color;
        std::string colorMap;
        unsigned char* colorMapData;
        int colorMapDataSize;
        float roughness;
        MaterialPackage()
        {
            color = new float[3];
            color[0] =  color[1] = color[2] = 1.0;
            colorMap = "";
            roughness = 0.23;
        }
        ~MaterialPackage()
        {
            delete color;
            delete colorMapData;
        }
    };
    */
	struct Node
	{
        virtual ~Node() {}
        std::string name;
		float position[3];
		float rotation[4];
		float scale[3];
        int childCount;
        bool editable;
	};

	struct NodeGeo : Node
	{
        NodeGeo() :
            geoId(-1),
            textureId(-1) {}
		int geoId;
		int textureId;
		float color[3];
		float roughness;
	};

	struct NodeLight : Node
	{
        NodeLight():
            type(SPOT),
            intensity(1.0),
            angle(60.0),
            color {1.0,1.0,1.0},
            exposure(3.0)
        {}
        int type;
        float color[3];
		float intensity;
		float angle;
        float exposure;
	};

	struct NodeCam : Node
	{
        NodeCam():
            fov(70),
            near(0.1),
            far(1000)
        {}
        float fov;
        float near;
        float far;
	};

	struct ObjectPackage
	{
        ObjectPackage() :
            dagpath(""),
            instanceId(""){}
		std::string dagpath;
        std::string instanceId;
		std::vector<float> vertices;
		std::vector<int> indices;
		std::vector<float> normals;
		std::vector<float> uvs;       
	};

	struct TexturePackage
	{
		std::string path;
		int colorMapDataSize;
		unsigned char* colorMapData;
	};

	struct SceneDistributorPluginState
	{
        SceneDistributorPluginState():
            numLights(0),
            numCameras(0),
            numObjectNodes(0)
        {}

		~SceneDistributorPluginState()
		{
			for (int i = 0; i < nodeList.size(); i++)
			{
				delete nodeList[i];
			}
			nodeList.clear();
            /*

			for (int i = 0; i < objPackList.size(); i++)
			{
				delete objPackList[i];
			}
			objPackList.clear();

			for (int i = 0; i < texPackList.size(); i++)
			{
				delete texPackList[i];
			}
			texPackList.clear();
            */
		}
		
		// Distribution Handling
        LodMode lodMode;
        std::string lodTag;

        // Data holder
        std::vector<Node*> nodeList;
        std::vector<ObjectPackage> objPackList;
		std::vector<TexturePackage> texPackList;

        // Currently processed node
        Node* node;

		// Materials
		FnAttribute::StringAttribute materialTerminalNamesAttr;

        // For stats
        int numLights;
        int numCameras;
        int numObjectNodes;

	};

    const int sizeof_node = 3* sizeof(float) + 4* sizeof(float) + 3* sizeof(float) + sizeof(int)+ sizeof(bool);
    const int sizeof_nodegeo = sizeof_node + sizeof(int) + sizeof(int) + 3* sizeof(float) + sizeof(float);
    const int sizeof_nodelight = sizeof_node + sizeof(int) + 3* sizeof(float) + sizeof(float) + sizeof(float) + sizeof(float);
    const int sizeof_nodecam = sizeof_node + sizeof(float) + sizeof(float) + sizeof(float);

}
}

#endif // COMPANYNAMEKATANA_PLUGINSTATE_H

/*
-----------------------------------------------------------------------------
This source file is part of VPET - Virtual Production Editing Tools
http://vpet.research.animationsinstitut.de/
http://github.com/FilmakademieRnd/VPET

Copyright (c) 2020 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

This project has been initiated in the scope of the EU funded project
Dreamspace under grant agreement no 610005 in the years 2014, 2015 and 2016.
http://dreamspaceproject.eu/
Post Dreamspace the project has been further developed on behalf of the
research and development activities of Animationsinstitut.

The VPET component Unreal Scene Distribution is intended for research and development
purposes only. Commercial use of any kind is not permitted.

There is no support by Filmakademie. Since the Unreal Scene Distribution is available
for free, Filmakademie shall only be liable for intent and gross negligence;
warranty is limited to malice. Scene DistributiorUSD may under no circumstances
be used for racist, sexual or any illegal purposes. In all non-commercial
productions, scientific publications, prototypical non-commercial software tools,
etc. using the Unreal Scene Distribution Filmakademie has to be named as follows:
“VPET-Virtual Production Editing Tool by Filmakademie Baden-Württemberg,
Animationsinstitut (http://research.animationsinstitut.de)“.

In case a company or individual would like to use the Unreal Scene Distribution in
a commercial surrounding or for commercial purposes, software based on these
components or any part thereof, the company/individual will have to contact
Filmakademie (research<at>filmakademie.de).
-----------------------------------------------------------------------------
*/
#pragma once

#include <vector>

namespace VPET
{
	enum LodMode { ALL, TAG };
	enum NodeType { GROUP, GEO, LIGHT, CAMERA };
	enum LightType { SPOT, DIRECTIONAL, POINT, AREA, NONE };

	//#pragma pack(4)
	struct Node
	{
		bool editable = false;
		int childCount = 0;
		float position[3] = { 1, 2, 3 };
		float scale[3] = { 1, 2, 3 };
		float rotation[4] = { 1, 2, 3, 4 };
		char name[64] = { ' ' };
	};

	struct NodeGeo : Node
	{
		int geoId = -1;
		int textureId = -1;
		int materialId = -1;
		float roughness = 0.166f;
		float color[4] = { 1,1,1,1 };
	};

	struct NodeLight : Node
	{
		int type = SPOT;
		float intensity = 1.0;
		float angle = 60.0;
		float range = 500.0;
		float exposure = 3.0;
		float color[3] = { 1.0,1.0,1.0 };
	};

	struct NodeCam : Node
	{
		float cFov = 70;
		float cNear = 1.0f;
		float cFar = 1000;
	};

	struct ObjectPackage
	{
		ObjectPackage() :
			dagpath(""),
			instanceId("")
		{}
		std::string dagpath;
		std::string instanceId;
		std::vector<float> vertices;
		std::vector<int> indices;
		std::vector<float> normals;
		std::vector<float> uvs;
		std::vector<float> boneWeights;
		std::vector<int> boneIndices;
	};

	struct TexturePackage
	{
		std::string path;
		int colorMapDataSize;
		unsigned char* colorMapData;
	};

	//#pragma pack(4)
	struct VpetHeader
	{
		float lightIntensityFactor = 1.0;
		int textureBinaryType = 0;
	};

	class SceneDistributorState
	{
	public:
		SceneDistributorState() :
			numLights(0),
			numCameras(0),
			numObjectNodes(0),
			textureBinaryType(0)
		{}

		~SceneDistributorState()
		{
			for (int i = 0; i < nodeList.size(); i++) {
				delete nodeList[i];
			}
			nodeList.clear();
		}

	public:
		// Distribution Handling
		LodMode lodMode;
		std::string lodTag;

		// Data
		VpetHeader vpetHeader;
		std::vector<Node*> nodeList;
		std::vector<NodeType> nodeTypeList;
		std::vector<ObjectPackage> objPackList;
		std::vector<TexturePackage> texPackList;
		int textureBinaryType;

		// Currently processed node
		Node* node;

		// For stats
		int numLights;
		int numCameras;
		int numObjectNodes;
	};

	// struct sizes 
	static const int sizeof_node = sizeof(Node);
	static const int sizeof_nodegeo = sizeof(NodeGeo);
	static const int sizeof_nodelight = sizeof(NodeLight);
	static const int sizeof_nodecam = sizeof(NodeCam);
}


/*
Copyright (c) 2020 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab
*/
#pragma once

#include <vector>

namespace VPET
{
	enum LodMode { ALL, TAG }; // is needed?
	enum NodeType { GROUP, GEO, LIGHT, CAMERA, SKINNEDMESH };
	enum LightType { SPOT, DIRECTIONAL, POINT, AREA, RECTANGLE, DISC, NONE };

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
		int materialId = -1;
		//float roughness = 0.166f;
		float color[4] = { 1,1,1,1 };
	};

	struct NodeLight : Node
	{
		int type = SPOT;
		float intensity = 1.0;
		float angle = 60.0;
		float range = 500.0;
		float color[3] = { 1.0,1.0,1.0 };
	};

	struct NodeCam : Node
	{
		float fov = 70;
		float aspect = 2;
		float nearPlane = 1.0f;
		float farPlane = 1000;
		float focalDist = 5;
		float aperture = 2;
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
		//std::string path;
		int colorMapDataSize;
		int width;
		int height;
		// Texture format?
		// Texture?
		unsigned char* colorMapData;
	};

	struct MaterialPackage
	{
		int type = 0; //! The type of the material. 0=standard, 1=load by name, 2=new with shader by name,  3=new with shader from source, 4= .. 
		std::string name;
		std::string src;
		int materialId = -1;
		std::vector<int> textureIds;
		//std::vector<int> textureNameIds;
		std::vector<float> textureOffsets;
		std::vector<float> textureScales;
		std::vector<bool> shaderConfig;
		std::vector<int> shaderPropertyIds;
		std::vector<int> shaderPropertyTypes;
		//std::vector<char> shaderProperties; // maybe unsigned char* ?
		//std::vector<float> shaderProperties;
		std::vector<char> shaderProperties;
	};

	struct VpetHeader
	{
		float lightIntensityFactor = 1.0;
		//int textureBinaryType = 0;
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
		std::vector<MaterialPackage> matPackList;
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


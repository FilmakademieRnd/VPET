/*
-----------------------------------------------------------------------------
This source file is part of VPET - Virtual Production Editing Tool
http://vpet.research.animationsinstitut.de/
http://github.com/FilmakademieRnd/VPET

Copyright (c) 2020 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

This project has been initiated in the scope of the EU funded project
Dreamspace under grant agreement no 610005 in the years 2014, 2015 and 2016.
http://dreamspaceproject.eu/
Post Dreamspace the project has been further developed on behalf of the
research and development activities of Animationsinstitut.
The SceneDistributiorUSD module was realized in 2019 and 2020 with funding from the EU funded project SAUCE under grant agreement no 780470

The VPET components SceneDistributiorUSD is intended for research and development
purposes only. Commercial use of any kind is not permitted.

There is no support by Filmakademie. Since the SceneDistributiorUSD is available
for free, Filmakademie shall only be liable for intent and gross negligence;
warranty is limited to malice. Scene DistributiorUSD may under no circumstances
be used for racist, sexual or any illegal purposes. In all non-commercial
productions, scientific publications, prototypical non-commercial software tools,
etc. using the SceneDistributiorUSD Filmakademie has to be named as follows:
“VPET-Virtual Production Editing Tool by Filmakademie Baden-Württemberg,
Animationsinstitut (http://research.animationsinstitut.de)“.

In case a company or individual would like to use the SceneDistributiorUSD in
a commercial surrounding or for commercial purposes, software based on these
components or any part thereof, the company/individual will have to contact
Filmakademie (research<at>filmakademie.de).
-----------------------------------------------------------------------------
*/

#include "SceneDistributor.h"

#include "pxr/usd/usd/stage.h"
#include "pxr/usd/usdGeom/mesh.h"
#include "pxr/usd/usdGeom/camera.h"
#include "pxr/usd/usdLux/light.h"
#include "pxr/usd/usdShade/materialBindingAPI.h"
#include "pxr/usd/ar/filesystemAsset.h"
#include "pxr/base/gf/rotation.h"
#include "pxr/usd/sdf/types.h"

#include <iostream>


PXR_NAMESPACE_USING_DIRECTIVE
namespace VPET
{
	std::atomic_bool m_stopThread(false);

	SceneDistributor::SceneDistributor(const std::string &pathName)
	{
		start(pathName);
	}
	
	SceneDistributor::~SceneDistributor()
	{
		m_stopThread = true;
	}

	void SceneDistributor::start(const std::string &pathName)
	{
		// get header values
		m_state.vpetHeader.lightIntensityFactor = 1.0;
		m_state.vpetHeader.textureBinaryType = 0;

		// set init to all
		m_state.lodMode = ALL;

		// set tagging mopde (from Katana)
		m_state.lodMode = TAG;
		m_state.lodTag = "lo";

		std::cout << "[INFO SceneDistributor] LodMode: " << m_state.lodMode << "  LodTag: " << m_state.lodTag << std::endl;
		std::cout << "[INFO SceneDistributor] Building scene..." << std::endl;

		UsdStageRefPtr stage = UsdStage::Open(pathName);

		if (!stage) {
			std::cout << pathName << " is not a valid USD file.";
			return;
		}

		UsdPrim root = stage->GetPseudoRoot();

		// traverse the scene graph
		buildLocation(&root);

		// Print stats
		std::cout << "[INFO SceneDistributorPlugin.start] Texture Count: " << m_state.texPackList.size() << std::endl;
		std::cout << "[INFO SceneDistributorPlugin.start] Object(Mesh) Count: " << m_state.objPackList.size() << std::endl;
		std::cout << "[INFO SceneDistributorPlugin.start] Node Count: " << m_state.nodeList.size() << std::endl;
		std::cout << "[INFO SceneDistributorPlugin.start] Objects: " << m_state.numObjectNodes << std::endl;
		std::cout << "[INFO SceneDistributorPlugin.start] Lights: " << m_state.numLights << std::endl;
		std::cout << "[INFO SceneDistributorPlugin.start] Cameras: " << m_state.numCameras << std::endl;

		//initalize zeroMQ thread
		std::cout << "Starting zeroMQ thread." << std::endl;
		std::thread t(server, &m_state);
		
		t.join();
		
		std::cout << "zeroMQ thread started." << std::endl;
	}

	static void* server(void *scene)
	{
		SceneDistributorState* m_sharedState = static_cast<SceneDistributorState*>(scene);

		std::cout << "Thread started. " << std::endl;

		zmq::context_t* context = new zmq::context_t(1);
		zmq::socket_t* socket = new zmq::socket_t(*context, ZMQ_REP);
		socket->bind("tcp://*:5565");
		std::cout << "zeroMQ running, now entering while." << std::endl;

		while (!m_stopThread)
		{
			zmq::message_t message;
			char* responseMessageContent;
			char* messageStart = NULL;
			int responseLength = 0;
			socket->recv(&message);

			std::string msgString;
			const char* msgPointer = static_cast<const char*>(message.data());
			if (msgPointer == NULL)
			{
				std::cout << "[INFO SceneDistributorPlugin.server] Error msgPointer is NULL" << std::endl;
			}
			else
			{
				msgString = std::string(static_cast<char*>(message.data()), message.size());
			}

			std::cout << "[INFO SceneDistributorPlugin.server] Got request string: " << msgString << std::endl;

			if (msgString == "header")
			{
				std::cout << "[INFO SceneDistributorPlugin.server] Got Header Request" << std::endl;
				responseLength = sizeof(VpetHeader);
				messageStart = responseMessageContent = (char*)malloc(responseLength);
				memcpy(responseMessageContent, (char*)&(m_sharedState->vpetHeader), sizeof(VpetHeader));
			}
			else if (msgString == "objects")
			{
				std::cout << "[INFO SceneDistributorPlugin.server] Got Objects Request" << std::endl;
				std::cout << "[INFO SceneDistributorPlugin.server] Object count " << m_sharedState->objPackList.size() << std::endl;
				responseLength = sizeof(int) * 5 * m_sharedState->objPackList.size();
				for (int i = 0; i < m_sharedState->objPackList.size(); i++)
				{
					responseLength += sizeof(float) * m_sharedState->objPackList[i].vertices.size();
					responseLength += sizeof(int) * m_sharedState->objPackList[i].indices.size();
					responseLength += sizeof(float) * m_sharedState->objPackList[i].normals.size();
					responseLength += sizeof(float) * m_sharedState->objPackList[i].uvs.size();
					responseLength += sizeof(float) * m_sharedState->objPackList[i].boneWeights.size();
					responseLength += sizeof(int) * m_sharedState->objPackList[i].boneIndices.size();
				}

				messageStart = responseMessageContent = (char*)malloc(responseLength);

				for (int i = 0; i < m_sharedState->objPackList.size(); i++)
				{
					// vSize
					int numValues = m_sharedState->objPackList[i].vertices.size() / 3.0;
					memcpy(responseMessageContent, (char*)&numValues, sizeof(int));
					responseMessageContent += sizeof(int);
					// vertices
					memcpy(responseMessageContent, &m_sharedState->objPackList[i].vertices[0], sizeof(float) * m_sharedState->objPackList[i].vertices.size());
					responseMessageContent += sizeof(float) * m_sharedState->objPackList[i].vertices.size();
					// iSize
					numValues = m_sharedState->objPackList[i].indices.size();
					memcpy(responseMessageContent, (char*)&numValues, sizeof(int));
					responseMessageContent += sizeof(int);
					// indices
					memcpy(responseMessageContent, &m_sharedState->objPackList[i].indices[0], sizeof(int) * m_sharedState->objPackList[i].indices.size());
					responseMessageContent += sizeof(int) * m_sharedState->objPackList[i].indices.size();
					// nSize
					numValues = m_sharedState->objPackList[i].normals.size() / 3.0;
					memcpy(responseMessageContent, (char*)&numValues, sizeof(int));
					responseMessageContent += sizeof(int);
					// normals
					memcpy(responseMessageContent, &m_sharedState->objPackList[i].normals[0], sizeof(float) * m_sharedState->objPackList[i].normals.size());
					responseMessageContent += sizeof(float) * m_sharedState->objPackList[i].normals.size();
					// uSize
					numValues = m_sharedState->objPackList[i].uvs.size() / 2.0;
					memcpy(responseMessageContent, (char*)&numValues, sizeof(int));
					responseMessageContent += sizeof(int);
					// uvs
					memcpy(responseMessageContent, &m_sharedState->objPackList[i].uvs[0], sizeof(float) * m_sharedState->objPackList[i].uvs.size());
					responseMessageContent += sizeof(float) * m_sharedState->objPackList[i].uvs.size();
					// bWSize
					numValues = m_sharedState->objPackList[i].boneWeights.size() / 4.0;
					memcpy(responseMessageContent, (char*)&numValues, sizeof(int));
					responseMessageContent += sizeof(int);
					// bone Weights
					memcpy(responseMessageContent, &m_sharedState->objPackList[i].boneWeights[0], sizeof(float) * m_sharedState->objPackList[i].boneWeights.size());
					responseMessageContent += sizeof(float) * m_sharedState->objPackList[i].boneWeights.size();
					// bone Intices
					memcpy(responseMessageContent, &m_sharedState->objPackList[i].boneIndices[0], sizeof(int) * m_sharedState->objPackList[i].boneIndices.size());
					responseMessageContent += sizeof(int) * m_sharedState->objPackList[i].boneIndices.size();
				}

			}
			else if (msgString == "textures")
			{
				std::cout << "[INFO SceneDistributorPlugin.server] Got Textures Request" << std::endl;
				std::cout << "[INFO SceneDistributorPlugin.server] Texture count " << m_sharedState->texPackList.size() << std::endl;

				responseLength = sizeof(int) + sizeof(int)*m_sharedState->texPackList.size();
				for (int i = 0; i < m_sharedState->texPackList.size(); i++)
				{
					responseLength += m_sharedState->texPackList[i].colorMapDataSize;
				}

				messageStart = responseMessageContent = (char*)malloc(responseLength);

				// texture binary type (image data (0) or raw unity texture data (1))
				int textureBinaryType = m_sharedState->textureBinaryType;
				std::cout << " textureBinaryType: " << textureBinaryType << std::endl;
				memcpy(responseMessageContent, (char*)&textureBinaryType, sizeof(int));
				responseMessageContent += sizeof(int);

				for (int i = 0; i < m_sharedState->texPackList.size(); i++)
				{
					memcpy(responseMessageContent, (char*)&m_sharedState->texPackList[i].colorMapDataSize, sizeof(int));
					responseMessageContent += sizeof(int);
					memcpy(responseMessageContent, m_sharedState->texPackList[i].colorMapData, m_sharedState->texPackList[i].colorMapDataSize);
					responseMessageContent += m_sharedState->texPackList[i].colorMapDataSize;
				}
			}
			else if (msgString == "nodes")
			{

				std::cout << "[INFO SceneDistributorPlugin.server] Got Nodes Request" << std::endl;
				std::cout << "[INFO SceneDistributorPlugin.server] Node count " << m_sharedState->nodeList.size() << " Node Type count " << m_sharedState->nodeList.size() << std::endl;

				// set the size from type- and namelength
				responseLength = sizeof(NodeType) * m_sharedState->nodeList.size();

				// extend with sizeof node depending on node type
				for (int i = 0; i < m_sharedState->nodeList.size(); i++)
				{

					if (m_sharedState->nodeTypeList[i] == NodeType::GEO)
						responseLength += sizeof_nodegeo;
					else if (m_sharedState->nodeTypeList[i] == NodeType::LIGHT)
						responseLength += sizeof_nodelight;
					else if (m_sharedState->nodeTypeList[i] == NodeType::CAMERA)
						responseLength += sizeof_nodecam;
					else
						responseLength += sizeof_node;

				}

				// allocate memory for out byte stream
				messageStart = responseMessageContent = (char*)malloc(responseLength);

				// iterate over node list copy data to out byte stream
				for (int i = 0; i < m_sharedState->nodeList.size(); i++)
				{
					Node* node = m_sharedState->nodeList[i];

					// First Copy node type
					int nodeType = m_sharedState->nodeTypeList[i];
					memcpy(responseMessageContent, (char*)&nodeType, sizeof(int));
					responseMessageContent += sizeof(int);

					// Copy specific node data
					if (m_sharedState->nodeTypeList[i] == NodeType::GEO)
					{
						memcpy(responseMessageContent, node, sizeof_nodegeo);
						responseMessageContent += sizeof_nodegeo;
					}
					else if (m_sharedState->nodeTypeList[i] == NodeType::LIGHT)
					{
						memcpy(responseMessageContent, node, sizeof_nodelight);
						responseMessageContent += sizeof_nodelight;
					}
					else if (m_sharedState->nodeTypeList[i] == NodeType::CAMERA)
					{
						memcpy(responseMessageContent, node, sizeof_nodecam);
						responseMessageContent += sizeof_nodecam;
					}
					else
					{
						memcpy(responseMessageContent, node, sizeof_node);
						responseMessageContent += sizeof_node;
					}

				}

			}

			std::cout << "[INFO SceneDistributorPlugin.server] Send message length: " << responseLength << std::endl;
			zmq::message_t responseMessage((void*)messageStart, responseLength, NULL);
			socket->send(responseMessage);
		}
		
		std::cout << "Zmq Thread ended, closing socket..." << std::endl;
		delete socket;
		std::cout << "Deleting context..." << std::endl;
		delete context;

		return 0;
	}

	void SceneDistributor::buildLocation(UsdPrim *prim)
	{
		//UsdPrimRange range(root);
		std::cout << "Build: " << prim->GetName() << std::endl;

		// Get scenegraph location type
		std::string typeName = prim->GetTypeName().GetString();

		m_state.node = 0;

		if (typeName == "Mesh") {
			m_state.node = new NodeGeo();
			buildNode((NodeGeo*) m_state.node, prim);
			std::cout << "Found geo node ";
		}
		else if (typeName == "Camera") {
			m_state.node = new NodeCam();
			buildNode((NodeCam*)m_state.node, prim);
			std::cout << "Found camera node ";
		}
		else if (typeName.find("Light") != std::string::npos) {
			m_state.node = new NodeLight();
			buildNode((NodeLight*)m_state.node, prim);
			std::cout << "Found light node ";
		}
		else {
			m_state.node = new Node();
			m_state.nodeTypeList.push_back(NodeType::GROUP);
			std::cout << "Found empty type, create group node ";
		}

		// node name
		std::string name = prim->GetName();
		if (name == "/")
			name = "world";
		name = name.substr(0, 63);
		strcpy_s(m_state.node->name, name.c_str());
		std::cout << name << std::endl;

		GfMatrix4d rotMat, shearMat, projectionMat;
		rotMat.SetIdentity();
		shearMat.SetIdentity();
		projectionMat.SetIdentity();
		GfVec3d scale, translation, rotationI;
		scale.Set(1.0, 1.0, 1.0);
		translation.Set(0.0, 0.0, 0.0);
		double rotationW = 1.0;
		rotationI.Set(0.0, 0.0, 0.0);

		UsdAttribute xFormAttr = prim->GetAttribute(TfToken("xformOp:transform"));
		if (xFormAttr) {
			GfMatrix4d transformMat;
			xFormAttr.Get(&transformMat);

			//transformMat = 
			//	GfMatrix4d(	 *transformMat[0], -*transformMat[4], -*transformMat[8],  *transformMat[3],
			//				-*transformMat[1],  *transformMat[5],  *transformMat[9],  *transformMat[7],
			//				-*transformMat[2],  *transformMat[6],  *transformMat[10], *transformMat[11],
			//				-*transformMat[12], *transformMat[13], *transformMat[14], *transformMat[15]);

			transformMat.Factor(&rotMat, &scale, &shearMat, &translation, &projectionMat);
		}

		GfQuaternion rotQuat = rotMat.ExtractRotation().GetQuaternion();
		//GfQuaternion zQuat = GfQuaternion(1.0, GfVec3f(0, 1, 0));
		//rotQuat *= zQuat;
		rotationW = rotQuat.GetReal();
		rotationI = rotQuat.GetImaginary();

		m_state.node->position[0] = translation[0];
		m_state.node->position[1] = translation[1];
		m_state.node->position[2] = translation[2];

		m_state.node->rotation[0] = rotationI[0];
		m_state.node->rotation[1] = rotationI[1];
		m_state.node->rotation[2] = rotationI[2];
		m_state.node->rotation[3] = rotationW;

		m_state.node->scale[0] = scale[0];
		m_state.node->scale[1] = scale[1];
		m_state.node->scale[2] = scale[2];

		m_state.node->childCount = 0;

		const UsdPrimSiblingRange &childs = prim->GetChildren();
		for (UsdPrimSiblingIterator cIter = childs.begin(); cIter != childs.end(); cIter++) {
			/*if (m_state.lodMode == TAG)
			{
				FnAttribute::StringAttribute lodTagAttr = child.getAttribute("info.componentLodTag");
				if (lodTagAttr.isValid())
				{
					if (m_state.lodTag == lodTagAttr.getValue("unknown", false))
					{
						m_state.node->childCount++;
					}
				}
				else
				{
					m_state.node->childCount++;
				}
			}
			else
			{*/
			m_state.node->childCount++;
			/*}*/

			//FnAttribute::IntAttribute editAttr = sgIterator.getAttribute("dreamspace.editable");
			//if (editAttr.isValid())
			//{
			//	if (1 == editAttr.getValue(0, false))
			//	{
					m_state.node->editable = true;
			//	}
			//}
		}

		std::cout << "[INFO SceneDistributor.SceneIterator] Add Node: " << m_state.node->name << std::endl;
		m_state.nodeList.push_back(m_state.node);
		
		// Recurse to children
		for (UsdPrimSiblingIterator cIter = childs.begin(); cIter != childs.end(); cIter++)
		{
			buildLocation(&*cIter);
		}
	}

	void SceneDistributor::buildNode(NodeGeo *node, UsdPrim *prim)
	{
		m_state.node = node;
		m_state.nodeTypeList.push_back(NodeType::GEO);

		std::string instanceID = "";

		if (prim->IsInstance()) {
			
			instanceID = prim->GetPath().GetString();
			std::cout << "[INFO SceneDistributor.GeometryScenegraphLocationDelegate] instanceID : " << instanceID << std::endl;

			int i = 0;
			for (; i < m_state.objPackList.size(); ++i)
			{
				if (m_state.objPackList[i].instanceId == instanceID)
				{
					break;
				}
			}

			if (i < m_state.objPackList.size())
			{
				node->geoId = i;
				std::cout << "[INFO SceneDistributor.GeometryScenegraphLocationDelegate] Instantiate to: " << node->geoId << std::endl;
			}
		}
		
		// get mesh from primitive
		UsdGeomMesh mesh = UsdGeomMesh(*prim);

		if (!mesh) {
			std::cout << prim->GetName() << " is no point based primitive!" << std::endl;
			return;
		}

		if (node->geoId < 0)
		{
			// Create Unity Package
			ObjectPackage objPack;
			objPack.instanceId = instanceID;

			// Faces
			VtArray<int> faceVIndices, faceVCounts;
			mesh.GetFaceVertexIndicesAttr().Get(&faceVIndices, 0);
			mesh.GetFaceVertexCountsAttr().Get(&faceVCounts, 0);

			// Vertices / Points
			VtVec3fArray PData;
			UsdAttribute pointsAttr = mesh.GetPointsAttr();
			bool getPoints = pointsAttr.Get(&PData, 0);

			// Normal
			// 1st regular method
			VtVec3fArray NData;
			VtArray<int> NiData;
			UsdAttribute normalsAttr = mesh.GetNormalsAttr();
			bool gotNormals = normalsAttr.Get(&NData, 0);
			bool gotNormalIndices = false;
			// 2nd try alternative naming
			if (!gotNormals) {
				UsdGeomPrimvar normalPrimvar = mesh.GetPrimvar(UsdGeomTokens->normals);
				gotNormals = normalPrimvar.Get(&NData, 0);
				gotNormalIndices = normalPrimvar.GetIndices(&NiData, 0);
			}

			// todo if no normals found, search for prim var normals!

			// ST or UV
			VtVec2fArray STData;
			VtArray<int> STindices;
			UsdGeomPrimvar stPrimvar = mesh.GetPrimvar(TfToken("primvars:UVMap"));
			if (!stPrimvar)
				stPrimvar = mesh.GetPrimvar(TfToken("primvars:Texture_uv"));
			bool gotSTs = stPrimvar.Get(&STData, 0);
			bool gotSTIndices = false;
			
			if (!gotSTs) {
				stPrimvar = mesh.GetPrimvar(TfToken("primvars:st"));
				gotSTs = stPrimvar.Get(&STData, 0);
			}
			gotSTIndices = stPrimvar.GetIndices(&STindices, 0);  // .GetIndices because "primvars:st:indices" is not allowed

			// std::cout << "Prepare Geo" << std::endl;

			// Get indices, normals, uvs
			// Iter over polygon indices, convert n-gons to triangles and store indices pointing to the vertex data
			// rebuild normal- and uv-data arrays to get one normal/uv per vertex (using the same indices)
			int startIdx = 0;
			int endIdx = 0;
			if (gotNormals) // assume defined edges including hard edge and therefor do not share vertices
			{
				for (int poly = 0; poly < faceVCounts.size(); poly++)
				{
					endIdx = startIdx + faceVCounts[poly]-1;

					for (int i = startIdx + 1; i < endIdx; i++)
					{
						// point indices
						int pIdx1, pIdx2, pIdx3;
						pIdx1 = faceVIndices[startIdx];
						pIdx2 = faceVIndices[i];
						pIdx3 = faceVIndices[i+1];

						// point 1
						objPack.vertices.push_back(PData[pIdx1][0]);
						objPack.vertices.push_back(PData[pIdx1][1]);
						objPack.vertices.push_back(PData[pIdx1][2]);
						// point 2
						objPack.vertices.push_back(PData[pIdx2][0]);
						objPack.vertices.push_back(PData[pIdx2][1]);
						objPack.vertices.push_back(PData[pIdx2][2]);
						// point 3
						objPack.vertices.push_back(PData[pIdx3][0]);
						objPack.vertices.push_back(PData[pIdx3][1]);
						objPack.vertices.push_back(PData[pIdx3][2]);

						// indices
						objPack.indices.push_back(objPack.vertices.size() / 3 - 3);
						objPack.indices.push_back(objPack.vertices.size() / 3 - 2);
						objPack.indices.push_back(objPack.vertices.size() / 3 - 1);

						// normal indices
						int nIdx1, nIdx2, nIdx3;
						if (gotNormalIndices) {
							nIdx1 = NiData[startIdx];
							nIdx2 = NiData[i];
							nIdx3 = NiData[i+1];
						}
						else {
							nIdx1 = startIdx;
							nIdx2 = i;
							nIdx3 = i+1;
						}

						// normals for every vertex
						// n1
						objPack.normals.push_back(NData[nIdx1][0]);
						objPack.normals.push_back(NData[nIdx1][1]);
						objPack.normals.push_back(NData[nIdx1][2]);
						// n2
						objPack.normals.push_back(NData[nIdx2][0]);
						objPack.normals.push_back(NData[nIdx2][1]);
						objPack.normals.push_back(NData[nIdx2][2]);
						// n3
						objPack.normals.push_back(NData[nIdx3][0]);
						objPack.normals.push_back(NData[nIdx3][1]);
						objPack.normals.push_back(NData[nIdx3][2]);
						
						if (gotSTs) // get uvs
						{
							// same for UVs but two values per index
							// (use different index map (st.index))
							int uvIdx0, uvIdx1, uvIdx2;
							if (gotSTIndices) {
								uvIdx0 = STindices[startIdx];
								uvIdx1 = STindices[i];
								uvIdx2 = STindices[i+1];
							}
							else {
								uvIdx0 = startIdx;
								uvIdx1 = i;
								uvIdx2 = i+1;
							}

							// uv1
							objPack.uvs.push_back(STData[uvIdx0][0]);
							objPack.uvs.push_back(STData[uvIdx0][1]);

							// uv2
							objPack.uvs.push_back(STData[uvIdx1][0]);
							objPack.uvs.push_back(STData[uvIdx1][1]);

							// uv3
							objPack.uvs.push_back(STData[uvIdx2][0]);
							objPack.uvs.push_back(STData[uvIdx2][1]);
						}
					}
					startIdx = endIdx + 1;
				}

			}
			else // assume all edges soft and therefor share vertices
			{

				// Normal
				VtArray<GfVec3f> normals(PData.size() , GfVec3f(0.0));

				// Uv
				VtArray<GfVec2f> uvs(faceVIndices.size() , GfVec2f(0.0));

				// Get vertices
				for (int i = 0; i < PData.size(); i++)
				{
					// TODO: hardcoded handiness
					objPack.vertices.push_back(PData[i][0]);
					objPack.vertices.push_back(PData[i][2]);
					objPack.vertices.push_back(PData[i][1]);
				}

				for (int poly = 0; poly < faceVCounts.size(); poly++)
				{
					endIdx = startIdx + faceVCounts[poly] - 1;

					for (int i = startIdx + 1; i < endIdx; i++)
					{
						// point indices
						int pIdx1 = faceVIndices[startIdx];
						int pIdx3 = faceVIndices[i];
						int pIdx2 = faceVIndices[i+1];

						// indices
						objPack.indices.push_back(pIdx1);
						objPack.indices.push_back(pIdx2);
						objPack.indices.push_back(pIdx3);

						// face normal
						GfVec3f a = GfVec3f(PData[pIdx2][0], PData[pIdx2][1], PData[pIdx2][2]) - GfVec3f(PData[pIdx1][0], PData[pIdx1][1], PData[pIdx1][2]);
						GfVec3f b = GfVec3f(PData[pIdx3][0], PData[pIdx3][1], PData[pIdx3][2]) - GfVec3f(PData[pIdx2][0], PData[pIdx2][1], PData[pIdx2][2]);
						GfVec3f n = b ^ a;

						normals[pIdx1] += n;
						normals[pIdx2] += n;
						normals[pIdx3] += n;

						if (gotSTs) // get uvs
						{
							// same vor UVs but two values per index
							// use different index map (st.index)
							int uvIdx0, uvIdx1, uvIdx2;
							if (gotSTIndices) {
								uvIdx0 = STindices[startIdx];
								uvIdx1 = STindices[i];
								uvIdx2 = STindices[i + 1];
							}
							else { 
								uvIdx0 = startIdx;
								uvIdx1 = i;
								uvIdx2 = i+1;
							}

							uvs[uvIdx0] = GfVec2f(STData[uvIdx0][0], STData[uvIdx0][1]);
							uvs[uvIdx1] = GfVec2f(STData[uvIdx1][0], STData[uvIdx1][1]);
							uvs[uvIdx2] = GfVec2f(STData[uvIdx2][0], STData[uvIdx2][1]);
						}
					}
					startIdx = endIdx + 1;
				}

				// fill normals float array
				for (int i = 0; i < normals.size(); i++)
				{
					// TODO: hardcoded handiness
					GfVec3f n = normals[i];
					n.Normalize();
					objPack.normals.push_back(n[0]);
					objPack.normals.push_back(n[2]);
					objPack.normals.push_back(n[1]);
				}

				// fill uvs float array
				for (int i = 0; i < uvs.size(); i++)
				{
					objPack.uvs.push_back(uvs[i][0]);
					objPack.uvs.push_back(uvs[i][1]);
				}

			}

			std::cout << "Point Count:" << objPack.vertices.size() / 3.0 << " Normal Count: " << objPack.normals.size() / 3.0 << " Vertex Count: " << objPack.indices.size() << std::endl;

			// store the object package
			m_state.objPackList.push_back(objPack);

			// get geo id
			node->geoId = m_state.objPackList.size() - 1;

		} // if ( nodeGeo->geoId < 0 )

		// get material
		UsdShadeMaterialBindingAPI::DirectBinding materialBindung = UsdShadeMaterialBindingAPI(mesh).GetDirectBinding();
		UsdShadeMaterial material = materialBindung.GetMaterial();

		if (material) {
			UsdShadeShader surfaceSource = material.ComputeSurfaceSource();
			
			surfaceSource.GetInput(TfToken("roughness")).Get(&node->roughness, 0);
			
			GfVec3f diffuseColor;
			UsdShadeInput diffuseInput = surfaceSource.GetInput(TfToken("diffuseColor"));
			
			if (diffuseInput.HasConnectedSource()) {
				UsdShadeConnectableAPI diffuseSource;
				TfToken diffuseSourceName;
				UsdShadeAttributeType diffuseSourceType;

				if (diffuseInput.GetConnectedSource(&diffuseSource, &diffuseSourceName, &diffuseSourceType)) {
					if (diffuseSource.IsShader()) {
						SdfAssetPath assetPath;
						std::vector<UsdShadeInput> inputs = diffuseSource.GetInputs();
						if (diffuseSource.GetInput(TfToken("file")).Get(&assetPath)) {
							
							std::string filePath = assetPath.GetResolvedPath();

							// check extension for "png"
							int posDel = filePath.rfind(".");
							
							if (filePath.substr(posDel + 1) != "jpg") 
								filePath = filePath.substr(0, posDel + 1) + "jpg";

							std::cout << "Map: " << filePath << std::endl;

							VPET::TexturePackage texPack;
							texPack.path = filePath;

							int i = 0;
							for (; i < m_state.texPackList.size(); ++i)
								if (m_state.texPackList[i].path == filePath)
									break;

							if (i < m_state.texPackList.size())
								node->textureId = i;
							else {
								// try load the image
								if (!LoadMap(filePath, texPack.colorMapData, &texPack.colorMapDataSize))
									std::cout << "Error reading map" << std::endl;
								else {
									std::cout << "Done reading map" << std::endl;
									m_state.texPackList.push_back(texPack);
									node->textureId = m_state.texPackList.size() - 1;
								}
							}
						}
					}
				}
			}
			else {
				diffuseInput.Get(&diffuseColor, 0);

				node->color[0] = diffuseColor[0];
				node->color[1] = diffuseColor[1];
				node->color[2] = diffuseColor[2];
			}

			
		}
		else {
			// prim color
			VtArray<GfVec3f> colorData;

			// prim opacity 
			float opacityData = 1.0f;

			if (mesh.GetPrimvar(UsdGeomTokens->primvarsDisplayColor).Get(&colorData, 0))
			{
				node->color[0] = colorData[0][0];
				node->color[1] = colorData[0][1];
				node->color[2] = colorData[0][2];

				if (mesh.GetPrimvar(UsdGeomTokens->primvarsDisplayOpacity).Get(&opacityData, 0))
					node->color[3] = opacityData;
			}
			else
			{
				// Set color to white
				node->color[0] = node->color[1] = node->color[2] = 1.0f;
			}
		}


		// store at sharedState to access it in iterator
		m_state.node = node;
		m_state.numObjectNodes++;
	}
	
	void SceneDistributor::buildNode(NodeCam *node, UsdPrim *prim)
	{
		m_state.node = node;
		m_state.nodeTypeList.push_back(NodeType::CAMERA);

		UsdGeomCamera camera = UsdGeomCamera(*prim);

		float focalLength, hAp, vAp;
		GfVec2f clippingRange;
		camera.GetFocalLengthAttr().Get(&focalLength);
		camera.GetHorizontalApertureAttr().Get(&hAp);
		camera.GetVerticalApertureAttr().Get(&vAp);
		camera.GetClippingRangeAttr().Get(&clippingRange);

		// Fov
		node->cFov = lensToVFov(focalLength, hAp);
		
		// Near
		node->cNear = clippingRange[0];
		
		// Far
		node->cFar = clippingRange[1];

		std::cout << "[INFO SceneDistributor.CameraScenegraphLocationDelegate] Camera FOV: " << node->cFov << " Near: " << node->cNear << " Far: " << node->cFar << std::endl;

		// store at sharedState to access it in iterator
		m_state.node = node;
		m_state.numCameras++;
	}
	
	void SceneDistributor::buildNode(NodeLight *node, UsdPrim *prim)
	{
		m_state.node = node;
		m_state.nodeTypeList.push_back(NodeType::LIGHT);

		UsdLuxLight light = UsdLuxLight(*prim);
		std::string typeName = prim->GetTypeName();
		
		GfVec3f color;
		light.GetColorAttr().Get(&color);
		node->color[0] = color[0];
		node->color[1] = color[1];
		node->color[2] = color[2];
		float intensity;
		light.GetIntensityAttr().Get(&node->intensity);
		float exposure;
		light.GetExposureAttr().Get(&node->exposure);

		if (typeName == "SphereLight"){
			node->type = VPET::POINT;
		}
		else if (typeName == "DistantLight") {
			node->type = VPET::DIRECTIONAL;
		}
		else if (typeName == "RectLight" || typeName == "DiscLight") {
			node->type = VPET::AREA;
			node->angle = 180;
		}
		else {
			UsdAttribute coneAngleAttr = prim->GetAttribute(UsdLuxTokens->shapingConeAngle);
			if (coneAngleAttr) {
				node->type = VPET::SPOT;
				float coneAgle;
				coneAngleAttr.Get(&node->angle);
			}
			else {
				node->type = VPET::NONE;
				delete node;
				Node* node = new Node();
				m_state.node = node;
				std::cout << "[INFO SceneDistributor.LightScenegraphLocationDelegate] Found unknown Light (add as group)" << std::endl;
				return;
			}
		}
		std::cout << "[INFO SceneDistributor.LightScenegraphLocationDelegate] Light color: " << node->color[0] << " " << node->color[1] << " " << node->color[2] << " Type: " << typeName << " intensity: " << node->intensity << " exposure: " << node->exposure << " coneAngle: " << node->angle << std::endl;

		// store at sharedState to access it in iterator
		m_state.node = node;
		m_state.numLights++;

	}
}
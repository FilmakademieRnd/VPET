// Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

#include "SceneSenderThread.h"

using namespace VPET;

// Distribution thread
void SceneSenderThread::DoWork()
{
	DOL(doLog, Warning, "[VPET2 DIST Thread] zeroMQ request-reply thread running");


	zmq::message_t message;
	std::string msgString;

	while (1)
	{
		char* responseMessageContent = NULL;
		char* messageStart = NULL;
		int responseLength = 0;

		// Blocking receive
		try {
			socket->recv(&message); // new flag for noblock: ZMQ_DONTWAIT
		}
		catch (const zmq::error_t& e)
		{
			FString errName = FString(zmq_strerror(e.num()));
			DOL(doLog, Error, "[VPET2 DIST Thread] recv exception: %s", *errName);
			return;
		}

		// Read message
		const char* msgPointer = static_cast<const char*>(message.data());
		if (msgPointer == NULL) {
			DOL(doLog, Error, "[VPET2 DIST Thread] Error msgPointer is NULL");
		}
		else {
			msgString = std::string(static_cast<char*>(message.data()), message.size());
		}

		FString fString(msgString.c_str());
		DOL(doLog, Log, "[DIST Thread] Got request string: %s", *fString);


		// Header request
		if (msgString == "header")
		{
			DOL(doLog, Log, "[DIST Thread] Got Header Request");
			responseLength = sizeof(VpetHeader);
			messageStart = responseMessageContent = (char*)malloc(responseLength);
			memcpy(responseMessageContent, (char*)&(m_sharedState->vpetHeader), sizeof(VpetHeader));
		}

		// Nodes request
		else if (msgString == "nodes")
		{
			DOL(doLog, Log, "[DIST Thread] Got Nodes Request");
			DOL(doLog, Log, "[DIST Thread] Node count: %d; Node Type count: %d", m_sharedState->nodeList.size(), m_sharedState->nodeList.size());

			// set the size from type- and name length
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

		// Objects request
		else if (msgString == "objects")
		{
			DOL(doLog, Log, "[DIST Thread] Got Objects Request");
			DOL(doLog, Log, "[DIST Thread] Object count: %d", m_sharedState->objPackList.size());

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
				// bone Indices
				memcpy(responseMessageContent, &m_sharedState->objPackList[i].boneIndices[0], sizeof(int) * m_sharedState->objPackList[i].boneIndices.size());
				responseMessageContent += sizeof(int) * m_sharedState->objPackList[i].boneIndices.size();
			}

		}

		// Characters request
		else if (msgString == "characters")
		{
			DOL(doLog, Log, "[DIST Thread] Got Characters Request");
		}

		// Textures request
		else if (msgString == "textures")
		{
			DOL(doLog, Log, "[DIST Thread] Got Textures Request");
			DOL(doLog, Log, "[DIST Thread] Texture count: %d", m_sharedState->texPackList.size());

			responseLength = 4 * sizeof(int) * m_sharedState->texPackList.size();
			for (int i = 0; i < m_sharedState->texPackList.size(); i++)
				responseLength += m_sharedState->texPackList[i].colorMapDataSize;

			messageStart = responseMessageContent = (char*)malloc(responseLength);

			for (int i = 0; i < m_sharedState->texPackList.size(); i++)
			{
				// width
				memcpy(responseMessageContent, (char*)&m_sharedState->texPackList[i].width, sizeof(int));
				responseMessageContent += sizeof(int);

				// height
				memcpy(responseMessageContent, (char*)&m_sharedState->texPackList[i].height, sizeof(int));
				responseMessageContent += sizeof(int);

				// format
				int format = 50; // for ASTC_RGB_6x6
				memcpy(responseMessageContent, (char*)&format, sizeof(int));
				responseMessageContent += sizeof(int);

				// data size
				memcpy(responseMessageContent, (char*)&m_sharedState->texPackList[i].colorMapDataSize, sizeof(int));
				responseMessageContent += sizeof(int);

				// pixel data
				memcpy(responseMessageContent, m_sharedState->texPackList[i].colorMapData, m_sharedState->texPackList[i].colorMapDataSize);
				responseMessageContent += m_sharedState->texPackList[i].colorMapDataSize;
			}

		}

		// Materials request
		else if (msgString == "materials")
		{
			DOL(doLog, Log, "[DIST Thread] Got Materials Request");

			// Prepare response
			responseLength = sizeof(int) * 8 * m_sharedState->matPackList.size();
			//DOL(doLog, Warning, "Response 0: %d", responseLength);
			for (int i = 0; i < m_sharedState->matPackList.size(); i++)
			{
				responseLength += m_sharedState->matPackList[i].name.size();
				//DOL(doLog, Warning, "Response 1: %d", responseLength);
				responseLength += m_sharedState->matPackList[i].src.size();
				//DOL(doLog, Warning, "Response 2: %d", responseLength);
				responseLength += sizeof(int) * m_sharedState->matPackList[i].textureIds.size();
				//DOL(doLog, Warning, "Response 3: %d", responseLength);
				responseLength += sizeof(float) * m_sharedState->matPackList[i].textureOffsets.size();
				//DOL(doLog, Warning, "Response 4: %d", responseLength);
				responseLength += sizeof(float) * m_sharedState->matPackList[i].textureScales.size();
				//DOL(doLog, Warning, "Response 5: %d", responseLength);
				responseLength += m_sharedState->matPackList[i].shaderConfig.size();
				//DOL(doLog, Warning, "Response 6: %d", responseLength);
				responseLength += sizeof(int) * m_sharedState->matPackList[i].shaderPropertyIds.size();
				//DOL(doLog, Warning, "Response 7: %d", responseLength);
				responseLength += sizeof(int) * m_sharedState->matPackList[i].shaderPropertyTypes.size();
				//DOL(doLog, Warning, "Response 8: %d", responseLength);
				responseLength += m_sharedState->matPackList[i].shaderProperties.size();
				//DOL(doLog, Warning, "Response 9: %d", responseLength);
			}

			// allocate
			messageStart = responseMessageContent = (char*)malloc(responseLength);

			// populate
			for (int i = 0; i < m_sharedState->matPackList.size(); i++)
			{
				// type (int)
				memcpy(responseMessageContent, (char*)&m_sharedState->matPackList[i].type, sizeof(int));
				responseMessageContent += sizeof(int);

				// name length (int)
				int nameLen = m_sharedState->matPackList[i].name.length();
				memcpy(responseMessageContent, (char*)&nameLen, sizeof(int));
				//DOL(doLog, Warning, "MATERIALDEV mat name length: %d", nameLen);
				responseMessageContent += sizeof(int);

				// name (byte[])
				memcpy(responseMessageContent, m_sharedState->matPackList[i].name.data(), nameLen);
				responseMessageContent += nameLen;

				// src length (int)
				int srcLen = m_sharedState->matPackList[i].src.length();
				memcpy(responseMessageContent, (char*)&srcLen, sizeof(int));
				//DOL(doLog, Warning, "MATERIALDEV src length: %d", srcLen);
				responseMessageContent += sizeof(int);

				// src (string)
				memcpy(responseMessageContent, m_sharedState->matPackList[i].src.data(), srcLen);
				responseMessageContent += srcLen;

				// matID (int)
				memcpy(responseMessageContent, (char*)&m_sharedState->matPackList[i].materialId, sizeof(int));
				responseMessageContent += sizeof(int);

				// size (int) for textureIds, textureOffsets/2 (Vec2), textureScales/2 (Vec2)
				int texIdSize = m_sharedState->matPackList[i].textureIds.size();
				memcpy(responseMessageContent, (char*)&texIdSize, sizeof(int));
				responseMessageContent += sizeof(int);

				// textureIds (int[])
				memcpy(responseMessageContent, &m_sharedState->matPackList[i].textureIds[0], sizeof(int) * m_sharedState->matPackList[i].textureIds.size());
				responseMessageContent += sizeof(int) * m_sharedState->matPackList[i].textureIds.size();

				// textureOffsets (float[])
				memcpy(responseMessageContent, &m_sharedState->matPackList[i].textureOffsets[0], sizeof(float) * m_sharedState->matPackList[i].textureOffsets.size());
				responseMessageContent += sizeof(float) * m_sharedState->matPackList[i].textureOffsets.size();

				// textureScales (float[])
				memcpy(responseMessageContent, &m_sharedState->matPackList[i].textureScales[0], sizeof(float) * m_sharedState->matPackList[i].textureScales.size());
				responseMessageContent += sizeof(float) * m_sharedState->matPackList[i].textureScales.size();

				// size shaderConfig (int)
				int shaderConfigSize = m_sharedState->matPackList[i].shaderConfig.size();
				memcpy(responseMessageContent, (char*)&shaderConfigSize, sizeof(int));
				responseMessageContent += sizeof(int);

				// shaderConfig (bool[])
				// accessing the address of the [0] leads to compiling issues, why?
				//memcpy(responseMessageContent, &m_sharedState->matPackList[i].shaderConfig[0], m_sharedState->matPackList[i].shaderConfig.size());
				//responseMessageContent += m_sharedState->matPackList[i].shaderConfig.size();
				// Current alternative sending false repeatedly
				bool tempbool = false;
				for (size_t j = 0; j < m_sharedState->matPackList[i].shaderConfig.size(); j++)
				{
					memcpy(responseMessageContent, &tempbool, sizeof(bool));
					responseMessageContent += sizeof(bool);
				}
				

				// size shader properties (int)
				int shaderPropertyIdsSize = m_sharedState->matPackList[i].shaderPropertyIds.size();
				memcpy(responseMessageContent, (char*)&shaderPropertyIdsSize, sizeof(int));
				responseMessageContent += sizeof(int);

				// shader property IDs (int[])
				memcpy(responseMessageContent, &m_sharedState->matPackList[i].shaderPropertyIds[0], sizeof(int) * m_sharedState->matPackList[i].shaderPropertyIds.size());
				responseMessageContent += sizeof(int) * m_sharedState->matPackList[i].shaderPropertyIds.size();

				// shader property types (int[])
				memcpy(responseMessageContent, &m_sharedState->matPackList[i].shaderPropertyTypes[0], sizeof(int) * m_sharedState->matPackList[i].shaderPropertyTypes.size());
				responseMessageContent += sizeof(int) * m_sharedState->matPackList[i].shaderPropertyTypes.size();

				// size shaderProperties data (int)
				int shaderPropertiesSize = m_sharedState->matPackList[i].shaderProperties.size();
				memcpy(responseMessageContent, (char*)&shaderPropertiesSize, sizeof(int));
				responseMessageContent += sizeof(int);

				// shader property data (byte[])
				memcpy(responseMessageContent, &m_sharedState->matPackList[i].shaderProperties[0], shaderPropertiesSize);
				responseMessageContent += shaderPropertiesSize;
			}
		}

		// Send subsequent zmq_send (needed due to ZMQ_REP type socket)
		DOL(doLog, Log, "[DIST Thread] Send message length: %d", responseLength);
		zmq::message_t responseMessage((void*)messageStart, responseLength, NULL);
		try {
			socket->send(responseMessage);
		}
		catch (const zmq::error_t& e)
		{
			FString errName = FString(zmq_strerror(e.num()));
			DOL(doLog, Error, "[DIST Thread] send exception: %s", *errName);
			return;
		}

		// In case of infinite while
		Sleep(10);
	}

}
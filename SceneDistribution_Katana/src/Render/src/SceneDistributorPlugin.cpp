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

#include "SceneDistributorPlugin.h"
#include "SceneIterator.h"

#include <FnScenegraphIterator/FnScenegraphIterator.h>
#include <FnRenderOutputUtils/FnRenderOutputUtils.h>
#include <FnRenderOutputUtils/CameraInfo.h>

#include <glm/mat4x4.hpp>
#include <glm/gtc/type_ptr.hpp>


#include <vector>

#include <sstream>


namespace Dreamspace
{
namespace Katana
{

SceneDistributorPlugin::SceneDistributorPlugin(
    FnKat::FnScenegraphIterator rootIterator,
    FnKat::GroupAttribute arguments)
    : FnKat::Render::RenderBase(rootIterator, arguments)
{
    // Query render settings
    /*int width, height;
    FnKat::RenderOutputUtils::getRenderResolution(rootIterator, &width, &height);

    // Get buffer manager handles
    FnKat::StringAttribute hostArgumentAttr = arguments.getChildByName("host");
    std::string bufferInfoAddress = hostArgumentAttr.getValue("", false);
    _sharedState.bufferInfo = FnConvertStringToBufferInfoPtr(bufferInfoAddress);
    CreateBufferFunc allocatebuffer = _sharedState.bufferInfo->_creatBufferFuncPtr;

    // Allocate buffer to output the rendered image
    _sharedState.bufferHandle = allocatebuffer(_sharedState.bufferInfo->_rendererInstance, width, height, 4);*/
}

SceneDistributorPlugin::~SceneDistributorPlugin()
{
}

int SceneDistributorPlugin::start()
{
    // Setup network material terminal names
    std::vector<std::string> terminalNames;
    // TODO: this should be openrlSurface and openrlLight
    terminalNames.push_back("prmanSurface");
    terminalNames.push_back("prmanLight");
    _sharedState.materialTerminalNamesAttr = FnAttribute::StringAttribute(terminalNames);

    FnKat::FnScenegraphIterator rootIterator = getRootIterator();

    // Process render settings
    /*int width, height;
    FnKat::RenderOutputUtils::getRenderResolution(rootIterator, &width, &height);
    std::cout << "[INFO SceneDistributor] Render resolution: " << width << "x" << height << std::endl;

    // Process camera info
    std::string cameraLocation = FnKat::RenderOutputUtils::getCameraPath(rootIterator);
    FnKat::RenderOutputUtils::CameraInfo cameraInfo = FnKat::RenderOutputUtils::getCameraInfo(rootIterator, cameraLocation);
    if (cameraInfo.isValid())
    {
        // Compute projection matrix
        float aspect = (cameraInfo.getRight() - cameraInfo.getLeft()) / (cameraInfo.getTop() - cameraInfo.getBottom());
        Imath::Frustumf frustum(cameraInfo.getNear(), cameraInfo.getFar(), cameraInfo.getFov() * 0.01745329252f, 0, aspect);
        frustum.setOrthographic(cameraInfo.getOrtho());
        _sharedState.projectionMatrix = frustum.projectionMatrix();
        std::cout << "[INFO SceneDistributor] Camera projection:" << std::endl << _sharedState.projectionMatrix << std::endl;

        // Get camera transform
        double *xform = cameraInfo.getXForm();
        Imath::M44f transform(xform[0], xform[1], xform[2], xform[3],
                              xform[4], xform[5], xform[6], xform[7],
                              xform[8], xform[9], xform[10], xform[11],
                              xform[12], xform[13], xform[14], xform[15]);
        _sharedState.viewMatrix = transform.inverse();
        std::cout << "[INFO SceneDistributor] Camera transform:" << std::endl << transform << std::endl;
    }*/

    // Traverse the scene graph
    FnKat::FnScenegraphIterator worldIterator = rootIterator.getChildByName("world");
    if (!worldIterator.isValid())
    {
        std::cerr << "[FATAL SceneDistributor] Could not find world." << std::endl;
        exit(-1);
    }


    // set init to all
    _sharedState.lodMode = ALL;
    // get values from root
    FnAttribute::StringAttribute lodModeAttr = rootIterator.getAttribute("sceneDistributorGlobalStatements.lod.mode");
    if ( lodModeAttr.isValid() )
    {
        std::string modeValue = lodModeAttr.getValue( "all", false );
        if ( modeValue == "by tag")
        {
            FnAttribute::StringAttribute lodTagAttr = rootIterator.getAttribute("sceneDistributorGlobalStatements.lod.selectionTag");
            if ( lodTagAttr.isValid() )
            {
                _sharedState.lodMode = TAG;
                _sharedState.lodTag = lodTagAttr.getValue( "lo", false );
            }
        }
    }

    std::cout << "[INFO SceneDistributor] LodMode: " << _sharedState.lodMode << "  LodTag: " << _sharedState.lodTag << std::endl;


    std::cout << "[INFO SceneDistributor] Building scene..." << std::endl;


    //glm::mat4 identity;
    //_sharedState.modelMatrices.push(identity);


    SceneIterator::buildLocation(worldIterator, &_sharedState);

    // Print stats
    std::cout << "[INFO SceneDistributorPlugin.start] Texture Count: " << _sharedState.texPackList.size() << std::endl;
    std::cout << "[INFO SceneDistributorPlugin.start] Object(Mesh) Count: " << _sharedState.objPackList.size() << std::endl;
    std::cout << "[INFO SceneDistributorPlugin.start] Node Count: " << _sharedState.nodeList.size() << std::endl;
    std::cout << "[INFO SceneDistributorPlugin.start] Objects: " << _sharedState.numObjectNodes << std::endl;
    std::cout << "[INFO SceneDistributorPlugin.start] Lights: " << _sharedState.numLights << std::endl;
    std::cout << "[INFO SceneDistributorPlugin.start] Cameras: " << _sharedState.numCameras << std::endl;


    // Prepare a single image block (RGBA)
    /*float dataBlock[] =
    {
        1.0f,0.4f,0.4f,1.0f, 1.0f,0.4f,0.4f,1.0f, 1.0f,0.4f,0.4f,1.0f, 1.0f,0.4f,0.4f,1.0f, 0.5f,1.0f,0.4f,1.0f, 0.5f,1.0f,0.4f,1.0f, 0.5f,1.0f,0.4f,1.0f, 0.5f,1.0f,0.4f,1.0f,
        1.0f,0.4f,0.4f,1.0f, 1.0f,0.4f,0.4f,1.0f, 1.0f,0.4f,0.4f,1.0f, 1.0f,0.4f,0.4f,1.0f, 0.5f,1.0f,0.4f,1.0f, 0.5f,1.0f,0.4f,1.0f, 0.5f,1.0f,0.4f,1.0f, 0.5f,1.0f,0.4f,1.0f,
        1.0f,0.4f,0.4f,1.0f, 1.0f,0.4f,0.4f,1.0f, 1.0f,0.4f,0.4f,1.0f, 1.0f,0.4f,0.4f,1.0f, 0.5f,1.0f,0.4f,1.0f, 0.5f,1.0f,0.4f,1.0f, 0.5f,1.0f,0.4f,1.0f, 0.5f,1.0f,0.4f,1.0f,
        1.0f,0.4f,0.4f,1.0f, 1.0f,0.4f,0.4f,1.0f, 1.0f,0.4f,0.4f,1.0f, 1.0f,0.4f,0.4f,1.0f, 0.5f,1.0f,0.4f,1.0f, 0.5f,1.0f,0.4f,1.0f, 0.5f,1.0f,0.4f,1.0f, 0.5f,1.0f,0.4f,1.0f,
        0.4f,0.5f,1.0f,1.0f, 0.4f,0.5f,1.0f,1.0f, 0.4f,0.5f,1.0f,1.0f, 0.4f,0.5f,1.0f,1.0f, 1.0f,0.8f,0.4f,1.0f, 1.0f,0.8f,0.4f,1.0f, 1.0f,0.8f,0.4f,1.0f, 1.0f,0.8f,0.4f,1.0f,
        0.4f,0.5f,1.0f,1.0f, 0.4f,0.5f,1.0f,1.0f, 0.4f,0.5f,1.0f,1.0f, 0.4f,0.5f,1.0f,1.0f, 1.0f,0.8f,0.4f,1.0f, 1.0f,0.8f,0.4f,1.0f, 1.0f,0.8f,0.4f,1.0f, 1.0f,0.8f,0.4f,1.0f,
        0.4f,0.5f,1.0f,1.0f, 0.4f,0.5f,1.0f,1.0f, 0.4f,0.5f,1.0f,1.0f, 0.4f,0.5f,1.0f,1.0f, 1.0f,0.8f,0.4f,1.0f, 1.0f,0.8f,0.4f,1.0f, 1.0f,0.8f,0.4f,1.0f, 1.0f,0.8f,0.4f,1.0f,
        0.4f,0.5f,1.0f,1.0f, 0.4f,0.5f,1.0f,1.0f, 0.4f,0.5f,1.0f,1.0f, 0.4f,0.5f,1.0f,1.0f, 1.0f,0.8f,0.4f,1.0f, 1.1f,0.8f,0.4f,1.0f, 1.0f,0.8f,0.4f,1.0f, 1.0f,0.8f,0.4f,1.0f
    };
    int entrysize = sizeof(float) * 4; // data stride
    int xmin, xmax_plusone, ymin, ymax_plusone; // rendered image coordinates
    // Loop through all blocks
    for (int i = 0; i < width / 8; ++i)
    {
        for (int j = 0; j < height / 8; ++j)
        {
            xmin = i * 8; xmax_plusone = (i + 1) * 8;
            ymin = j * 8; ymax_plusone = (j + 1) * 8;
            // Write image blocks to the buffer
            _sharedState.bufferInfo->_writeDataBlockFuncPtr(_sharedState.bufferHandle, xmin, xmax_plusone, ymin, ymax_plusone, entrysize, 1, reinterpret_cast<unsigned char*>(dataBlock));
        }
    }*/

    //initalize zeroMQ thread
    std::cout << "Starting zeroMQ thread." << std::endl;
    pthread_t thrd;
    // pthread_create(&thrd, NULL, &server, static_cast<void*>(&_sharedState.objPack));
	pthread_create(&thrd, NULL, &server, static_cast<void*>(&_sharedState));
	std::cout << "zeroMQ thread started." << std::endl;

    return 0;
}

int SceneDistributorPlugin::pause()
{
    // Default:
    return FnKat::Render::RenderBase::pause();
}

int SceneDistributorPlugin::resume()
{
    // Default:
    return FnKat::Render::RenderBase::resume();
}

int SceneDistributorPlugin::stop()
{
    // Default:
    return FnKat::Render::RenderBase::stop();
}

int SceneDistributorPlugin::startLiveEditing()
{
    // Default:
    return FnKat::Render::RenderBase::startLiveEditing();
}

int SceneDistributorPlugin::stopLiveEditing()
{
    // Default:
    return FnKat::Render::RenderBase::stopLiveEditing();
}

int SceneDistributorPlugin::processControlCommand(const std::string& command)
{
    // Default:
    return FnKat::Render::RenderBase::processControlCommand(command);
}

int SceneDistributorPlugin::queueDataUpdates(FnKat::GroupAttribute updateAttribute)
{
    // Loop through all updates
    for (int i = 0; i < updateAttribute.getNumberOfChildren(); i++)
    {
        FnAttribute::GroupAttribute commandAttr = updateAttribute.getChildByIndex(i);
        if (!commandAttr.isValid())
        {
            continue;
        }

        // Prepare data of a single update
        FnAttribute::StringAttribute typeAttr = commandAttr.getChildByName("type");
        FnAttribute::StringAttribute locationAttr = commandAttr.getChildByName("location");
        FnAttribute::GroupAttribute attributesAttr = commandAttr.getChildByName("attributes");

        std::string type = typeAttr.getValue("", false);
        std::string location = locationAttr.getValue("", false);

        SceneDistributorPlugin::Update update;
        update.type = type;
        update.location = location;
        update.attributesAttr = attributesAttr;

        // Queue updates by type
        if(type == "camera" && attributesAttr.isValid())
        {
            // TODO: make queue thread-safe
            _cameraUpdates.push(update);
        }
    }
}

int SceneDistributorPlugin::applyPendingDataUpdates()
{
    // Delegate updates
    /*if(!_cameraUpdates.empty())
    {
        updateCamera();
    }*/
    // Re-render frame
}

static void* server(void *scene)
{
	// std::vector<ObjectPackage> *objPack = static_cast<std::vector<ObjectPackage>*>(scene);
	SceneDistributorPluginState* sharedState = static_cast<SceneDistributorPluginState*>(scene);


    std::cout << "Thread started. " << std::endl;

    zmq::context_t* context = new  zmq::context_t(1);
	zmq::socket_t* socket = new zmq::socket_t(*context, ZMQ_REP);
	socket->bind("tcp://*:5565");
	std::cout << "zeroMQ running, now entering while." << std::endl;

	while (1)
	{
		zmq::message_t message;
		char* responseMessageContent;
		char* messageStart;
		int responseLength = 0;
        socket->recv(&message);
        std::string msgString(static_cast<char*>(message.data()));

        // std::cout << msgString << std::endl;

        if (msgString == "start")
		{
           /*
		  std::cout << "preparing initial message" << std::endl;
		  std::ostringstream msgStream;
		  msgStream << objPack->size();
		  messageStart = responseMessageContent = (char*)malloc(msgStream.str().size()+1);
		  std::strcpy(responseMessageContent, msgStream.str().c_str());
		  responseLength = msgStream.str().size()+1;
		  std::cout << msgStream.str() << std::endl;
		  std::cout << "prepared initial message" << std::endl;
          */
		}
        else if (msgString == "objec")
		{
            std::cout << "[INFO SceneDistributorPlugin.server] Got Objcts Request" << std::endl;
            std::cout << "[INFO SceneDistributorPlugin.server] Object count " << sharedState->objPackList.size() << std::endl;
            responseLength = sizeof(int) * 4 * sharedState->objPackList.size();
            for (int i = 0; i < sharedState->objPackList.size(); i++)
            {
                responseLength += sizeof(float) * sharedState->objPackList[i].vertices.size();
                responseLength += sizeof(int) * sharedState->objPackList[i].indices.size();
                responseLength += sizeof(float) * sharedState->objPackList[i].normals.size();
                responseLength += sizeof(float) * sharedState->objPackList[i].uvs.size();
            }

			messageStart = responseMessageContent = (char*)malloc(responseLength);

            for (int i = 0; i < sharedState->objPackList.size(); i++)
            {
                // vSize
                int numValues = sharedState->objPackList[i].vertices.size();
                memcpy(responseMessageContent, (char*)&numValues, sizeof(int));
                responseMessageContent += sizeof(int);
                // vertices
                memcpy(responseMessageContent, &sharedState->objPackList[i].vertices[0], sizeof(float) * sharedState->objPackList[i].vertices.size());
                responseMessageContent += sizeof(float) * sharedState->objPackList[i].vertices.size();
                // iSize
                numValues = sharedState->objPackList[i].indices.size();
                memcpy(responseMessageContent, (char*)&numValues, sizeof(int));
                responseMessageContent += sizeof(int);
                // indices
                memcpy(responseMessageContent, &sharedState->objPackList[i].indices[0], sizeof(int) * sharedState->objPackList[i].indices.size());
                responseMessageContent += sizeof(int) * sharedState->objPackList[i].indices.size();
                // nSize
                numValues = sharedState->objPackList[i].normals.size();
                memcpy(responseMessageContent, (char*)&numValues, sizeof(int));
                responseMessageContent += sizeof(int);
                // normals
                memcpy(responseMessageContent, &sharedState->objPackList[i].normals[0], sizeof(float) * sharedState->objPackList[i].normals.size());
                responseMessageContent += sizeof(float) * sharedState->objPackList[i].normals.size();
                // uSize
                numValues = sharedState->objPackList[i].uvs.size();
                memcpy(responseMessageContent, (char*)&numValues, sizeof(int));
                responseMessageContent += sizeof(int);
                // uvs
                memcpy(responseMessageContent, &sharedState->objPackList[i].uvs[0], sizeof(float) * sharedState->objPackList[i].uvs.size());
                responseMessageContent += sizeof(float) * sharedState->objPackList[i].uvs.size();
            }

		}
        else if (msgString == "textu")
		{
            std::cout << "[INFO SceneDistributorPlugin.server] Got Textures Request" << std::endl;
            std::cout << "[INFO SceneDistributorPlugin.server] Texture count " << sharedState->texPackList.size() << std::endl;
            responseLength = sizeof(int)*sharedState->texPackList.size();
            for (int i = 0; i < sharedState->texPackList.size(); i++)
            {
                responseLength += sharedState->texPackList[i].colorMapDataSize;
            }

			messageStart = responseMessageContent = (char*)malloc(responseLength);

            for (int i = 0; i < sharedState->texPackList.size(); i++)
			{
                memcpy(responseMessageContent, (char*)&sharedState->texPackList[i].colorMapDataSize, sizeof(int));
                responseMessageContent += sizeof(int);
                memcpy(responseMessageContent, sharedState->texPackList[i].colorMapData, sharedState->texPackList[i].colorMapDataSize);
                responseMessageContent += sharedState->texPackList[i].colorMapDataSize;
            }
		}
		else if (msgString == "nodes")
		{

            std::cout << "[INFO SceneDistributorPlugin.server] Got Nodes Request" << std::endl;
            std::cout << "[INFO SceneDistributorPlugin.server] Node count " << sharedState->nodeList.size() << std::endl;

            // set the size from type- and namelength
            responseLength =  2 * sizeof(int) * sharedState->nodeList.size();

            // extend with sizeof node depending on node type
			for (int i = 0; i < sharedState->nodeList.size(); i++)
			{
                Node* node = sharedState->nodeList[i];

                // name length
                responseLength += node->name.size();

                if (dynamic_cast<NodeGeo*>( node ))
                    responseLength += sizeof_nodegeo; //(NodeGeo);
                else if (dynamic_cast<NodeLight*>(node))
                    responseLength += sizeof_nodelight; //sizeof(NodeLight);
                else if (dynamic_cast<NodeCam*>(node))
                    responseLength += sizeof_nodecam; //sizeof(NodeCam);
				else 
                    responseLength += sizeof_node; //sizeof(Node);
			}

            // allocate memory for out byte stream
            messageStart = responseMessageContent = (char*)malloc(responseLength);

            // iterate over node list copy data to out byte stream
			for (int i = 0; i < sharedState->nodeList.size(); i++)
			{
                Node* node = sharedState->nodeList[i];
                NodeGeo* nodeGeo = dynamic_cast<NodeGeo*>(node);
                NodeLight* nodeLight = dynamic_cast<NodeLight*>(node);
                NodeCam* nodeCam = dynamic_cast<NodeCam*>(node);

                // First Copy node type
                int nodeType = GROUP;
                if ( nodeGeo )
                    nodeType = GEO;
                else if ( nodeLight )
                    nodeType = LIGHT;
                else if ( nodeCam )
                    nodeType = CAMERA;

                memcpy(responseMessageContent, (char*)&nodeType, sizeof(int));
                responseMessageContent += sizeof(int);

                // Copy basic node data ( group with name, transform, etc )
                int nameLength = node->name.size();
                memcpy(responseMessageContent, (char*)&nameLength, sizeof(int));
                responseMessageContent += sizeof(int);
                std::strcpy(responseMessageContent, node->name.c_str());
                responseMessageContent += nameLength;
                memcpy(responseMessageContent, (char*)node->position, 3 * sizeof(float));
                responseMessageContent += 3*sizeof(float);
                memcpy(responseMessageContent, (char*)node->rotation, 4 * sizeof(float));
                responseMessageContent += 4*sizeof(float);
                memcpy(responseMessageContent, (char*)node->scale, 3 * sizeof(float));
                responseMessageContent += 3*sizeof(float);
                memcpy(responseMessageContent, (char*)&node->childCount, sizeof(int));
                responseMessageContent += sizeof(int);
                memcpy(responseMessageContent, (char*)&node->editable, sizeof(bool));
                responseMessageContent += sizeof(bool);

                // Copy specific node data
				if (nodeGeo)
				{
                    memcpy(responseMessageContent, (char*)&nodeGeo->geoId, sizeof(int));
                    responseMessageContent += sizeof(int);
                    memcpy(responseMessageContent, (char*)&nodeGeo->textureId, sizeof(int));
                    responseMessageContent += sizeof(int);
                    memcpy(responseMessageContent, (char*)nodeGeo->color, 3 * sizeof(float));
                    responseMessageContent += 3*sizeof(float);
                    memcpy(responseMessageContent, (char*)&nodeGeo->roughness, sizeof(float));
                    responseMessageContent += sizeof(float);

				}
				else if (nodeLight)
				{
                    memcpy(responseMessageContent, (char*)&nodeLight->type, sizeof(int));
                    responseMessageContent += sizeof(int);
                    memcpy(responseMessageContent, (char*)nodeLight->color, 3 * sizeof(float));
                    responseMessageContent += 3*sizeof(float);
                    memcpy(responseMessageContent, (char*)&nodeLight->intensity, sizeof(float));
                    responseMessageContent += sizeof(float);
                    memcpy(responseMessageContent, (char*)&nodeLight->angle, sizeof(float));
                    responseMessageContent += sizeof(float);
                    memcpy(responseMessageContent, (char*)&nodeLight->exposure, sizeof(float));
                    responseMessageContent += sizeof(float);
                }
				else if (nodeCam)
				{
                    memcpy(responseMessageContent, (char*)&nodeCam->fov, sizeof(float));
                    responseMessageContent += sizeof(float);
                    memcpy(responseMessageContent, (char*)&nodeCam->near, sizeof(float));
                    responseMessageContent += sizeof(float);
                    memcpy(responseMessageContent, (char*)&nodeCam->far, sizeof(float));
                    responseMessageContent += sizeof(float);
                }

            }

		}

        std::cout << "[INFO SceneDistributorPlugin.server] Send message length: " << responseLength << std::endl;
        zmq::message_t responseMessage((void*)messageStart, responseLength, NULL);
        socket->send(responseMessage);

    }


	return 0;
}

bool SceneDistributorPlugin::hasPendingDataUpdates() const
{
    // Check for any queued updates
    return !_cameraUpdates.empty();
}

void SceneDistributorPlugin::updateCamera()
{
    // Apply all queued camera updates
    /*while(!_cameraUpdates.empty())
    {
        // Remove update from queue
        Update update = _cameraUpdates.front();
        _cameraUpdates.pop();

        // Get transform attribute
        FnAttribute::GroupAttribute xFormAttr = update.attributesAttr.getChildByName("xform");
        if (!xFormAttr.isValid())
            continue;

        // Update camera transform
        Imath::M44f transform;
        std::vector<FnKat::RenderOutputUtils::Transform> xFormList;
        FnKat::RenderOutputUtils::fillXFormListFromAttributes(xFormList, xFormAttr);
        for (std::vector<FnKat::RenderOutputUtils::Transform>::const_iterator xFormIt = xFormList.begin(); xFormIt != xFormList.end(); ++xFormIt)
        {
            FnKat::RenderOutputUtils::TransformList transformList = xFormIt->transformList;
            for (FnKat::RenderOutputUtils::TransformList::const_iterator transformIt = transformList.begin(); transformIt != transformList.end(); ++transformIt)
            {
                std::string transformType = transformIt->first;
                FnKat::RenderOutputUtils::TransformData transformData = transformIt->second;
    
                Imath::M44f singleTransform;
                if (transformType == "translate" && transformData.size() == 3)
                {
                    Imath::V3f translation(-transformData[0], -transformData[1], -transformData[2]);
                    singleTransform.translate(translation);
                }
                else if (transformType == "rotate" && transformData.size() == 4)
                {
                    Imath::V3f axis(transformData[1], transformData[2], transformData[3]);
                    Imath::M44f rotation;
                    rotation.setAxisAngle(axis, -transformData[0] * 0.01745329252f);
                    singleTransform *= rotation;
                }
                else if (transformType == "scale" && transformData.size() == 3)
                {
                    Imath::V3f scale(1.0f / transformData[0], 1.0f / transformData[1], 1.0f / transformData[2]);
                    singleTransform.scale(scale);
                }
                else if (transformType == "matrix" && transformData.size() == 16)
                {
                    Imath::M44f matrix(transformData[ 0], transformData[ 1], transformData[ 2], transformData[ 3],
                                       transformData[ 4], transformData[ 5], transformData[ 6], transformData[ 7],
                                       transformData[ 8], transformData[ 9], transformData[10], transformData[11],
                                       transformData[12], transformData[13], transformData[14], transformData[15]);
                    singleTransform = matrix.inverse();
                }
                transform *= singleTransform;
            }
        }
        // Keep cached camera transform up to date
        _sharedState.viewMatrix = transform.inverse();
        std::cout << "[INFO SceneDistributor] Camera transform update:" << std::endl << transform << std::endl;
   }*/
}

// Disk Render

void SceneDistributorPlugin::configureDiskRenderOutputProcess(
    FnKat::Render::DiskRenderOutputProcess& diskRenderOutputProcess,
    const std::string& outputName,
    const std::string& outputPath,
    const std::string& renderMethodName,
    const float& frameTime) const
{
    // e.g.

    // The render action used for this render output:
    std::auto_ptr<FnKat::Render::RenderAction> renderAction;

    // Set the render action to do nothing:
    renderAction.reset(new FnKat::Render::NoOutputRenderAction());

    // Pass ownership of the renderAction to the diskRenderOutputProcess:
    diskRenderOutputProcess.setRenderAction(renderAction);
}

DEFINE_RENDER_PLUGIN(SceneDistributorPlugin)

}
}

void registerPlugins()
{
    REGISTER_PLUGIN(Dreamspace::Katana::SceneDistributorPlugin, "sceneDistributor", 0, 1);
}

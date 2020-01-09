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

The VPET components Scene Distribution and Synchronization Server are intended
for research and development purposes only. Commercial use of any kind is not 
permitted.

There is no support by Filmakademie. Since the Scene Distribution and 
Synchronization Server are available for free, Filmakademie shall only be 
liable for intent and gross negligence; warranty is limited to malice. Scene 
Distribution and Synchronization Server may under no circumstances be used for 
racist, sexual or any illegal purposes. In all non-commercial productions, 
scientific publications, prototypical non-commercial software tools, etc. 
using the Scene Distribution and/or Synchronization Server Filmakademie has 
to be named as follows: “VPET-Virtual Production Editing Tool by Filmakademie 
Baden-Württemberg, Animationsinstitut (http://research.animationsinstitut.de)“.

In case a company or individual would like to use the Scene Distribution and/or 
Synchronization Server in a commercial surrounding or for commercial purposes, 
software based on these components or any part thereof, the company/individual 
will have to contact Filmakademie (research<at>filmakademie.de).
-----------------------------------------------------------------------------
*/
#include "SceneDistributorPlugin.h"
#include "SceneIterator.h"

#include <FnAPI\FnAPI.h>

#include <FnScenegraphIterator/FnScenegraphIterator.h>
#include <FnRenderOutputUtils/FnRenderOutputUtils.h>
#include <FnRenderOutputUtils/CameraInfo.h>

#include <glm/mat4x4.hpp>
#include <glm/gtc/type_ptr.hpp>

#include <vector>
#include <sstream>
#include <typeinfo>

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
    //terminalNames.push_back("prmanSurface");
    //terminalNames.push_back("prmanLight");
    _sharedState.materialTerminalNamesAttr = FnAttribute::StringAttribute(terminalNames);

    FnKat::FnScenegraphIterator rootIterator = getRootIterator();


    // get header values
    FnAttribute::FloatAttribute lightIntensityAttr = rootIterator.getAttribute("sceneDistributorGlobalStatements.lightIntensityFactor");
    if ( lightIntensityAttr.isValid())
    {

        _sharedState.vpetHeader.lightIntensityFactor = lightIntensityAttr.getValue(1.0, false);
    }
    else
    {
         std::cout << "[INFO SceneDistributor] ERROR Not found attribute: lightIntensityFactor" << std::endl;
    }

    FnAttribute::IntAttribute textureTypeAttr = rootIterator.getAttribute("sceneDistributorGlobalStatements.textureBinaryType");
    if ( textureTypeAttr.isValid())
    {

        _sharedState.vpetHeader.textureBinaryType = textureTypeAttr.getValue(0, false);
    }
    else
    {
         std::cout << "[INFO SceneDistributor] ERROR Not found attribute: textureBinaryType" << std::endl;
    }




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
	return 0;
}

int SceneDistributorPlugin::applyPendingDataUpdates()
{
    // Delegate updates
    /*if(!_cameraUpdates.empty())
    {
        updateCamera();
    }*/
    // Re-render frame

	return 0;
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

        std::string msgString;
        const char* msgPointer = static_cast<const char*>(message.data());
        if ( msgPointer == NULL )
        {
             std::cout << "[INFO SceneDistributorPlugin.server] Error msgPointer is NULL" << std::endl;
        }
        else
        {
            msgString = std::string( static_cast<char*>(message.data()), message.size() );
        }

        std::cout << "[INFO SceneDistributorPlugin.server] Got request string: " << msgString << std::endl;

        if (msgString == "header")
        {
            std::cout << "[INFO SceneDistributorPlugin.server] Got Header Request" << std::endl;
            responseLength =sizeof(VpetHeader);
            messageStart = responseMessageContent = (char*)malloc(responseLength);
            memcpy(responseMessageContent, (char*)&(sharedState->vpetHeader), sizeof(VpetHeader));
        }
        else if (msgString == "objects")
		{
            std::cout << "[INFO SceneDistributorPlugin.server] Got Objects Request" << std::endl;
            std::cout << "[INFO SceneDistributorPlugin.server] Object count " << sharedState->objPackList.size() << std::endl;
            responseLength = sizeof(int) * 5 * sharedState->objPackList.size();
            for (int i = 0; i < sharedState->objPackList.size(); i++)
            {
                responseLength += sizeof(float) * sharedState->objPackList[i].vertices.size();
                responseLength += sizeof(int) * sharedState->objPackList[i].indices.size();
                responseLength += sizeof(float) * sharedState->objPackList[i].normals.size();
				responseLength += sizeof(float) * sharedState->objPackList[i].uvs.size();
				responseLength += sizeof(float) * sharedState->objPackList[i].boneWeights.size();
				responseLength += sizeof(int) * sharedState->objPackList[i].boneIndices.size();
            }

            messageStart = responseMessageContent = (char*)malloc(responseLength);

            for (int i = 0; i < sharedState->objPackList.size(); i++)
            {
                // vSize
                int numValues = sharedState->objPackList[i].vertices.size()/3.0;
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
                numValues = sharedState->objPackList[i].normals.size()/3.0;
                memcpy(responseMessageContent, (char*)&numValues, sizeof(int));
                responseMessageContent += sizeof(int);
                // normals
                memcpy(responseMessageContent, &sharedState->objPackList[i].normals[0], sizeof(float) * sharedState->objPackList[i].normals.size());
                responseMessageContent += sizeof(float) * sharedState->objPackList[i].normals.size();
                // uSize
                numValues = sharedState->objPackList[i].uvs.size()/2.0;
                memcpy(responseMessageContent, (char*)&numValues, sizeof(int));
                responseMessageContent += sizeof(int);
                // uvs
                memcpy(responseMessageContent, &sharedState->objPackList[i].uvs[0], sizeof(float) * sharedState->objPackList[i].uvs.size());
                responseMessageContent += sizeof(float) * sharedState->objPackList[i].uvs.size();
				// bWSize
				numValues = sharedState->objPackList[i].boneWeights.size()/4.0;
				memcpy(responseMessageContent, (char*)&numValues, sizeof(int));
				responseMessageContent += sizeof(int);
				// bone Weights
				memcpy(responseMessageContent, &sharedState->objPackList[i].boneWeights[0], sizeof(float) * sharedState->objPackList[i].boneWeights.size());
				responseMessageContent += sizeof(float) * sharedState->objPackList[i].boneWeights.size();
				// bone Intices
				memcpy(responseMessageContent, &sharedState->objPackList[i].boneIndices[0], sizeof(int) * sharedState->objPackList[i].boneIndices.size());
				responseMessageContent += sizeof(int) * sharedState->objPackList[i].boneIndices.size();
            }

        }
        else if (msgString == "textures")
        {
            std::cout << "[INFO SceneDistributorPlugin.server] Got Textures Request" << std::endl;
            std::cout << "[INFO SceneDistributorPlugin.server] Texture count " << sharedState->texPackList.size() << std::endl;

            responseLength =  sizeof(int) + sizeof(int)*sharedState->texPackList.size();
            for (int i = 0; i < sharedState->texPackList.size(); i++)
            {
                responseLength += sharedState->texPackList[i].colorMapDataSize;
            }

            messageStart = responseMessageContent = (char*)malloc(responseLength);

            // texture binary type (image data (0) or raw unity texture data (1))
            int textureBinaryType = sharedState->textureBinaryType;
            std::cout << " textureBinaryType: " << textureBinaryType << std::endl;
            memcpy(responseMessageContent, (char*)&textureBinaryType, sizeof(int));
            responseMessageContent += sizeof(int);

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
            std::cout << "[INFO SceneDistributorPlugin.server] Node count " << sharedState->nodeList.size() << " Node Type count " << sharedState->nodeList.size() << std::endl;

            // set the size from type- and namelength
            responseLength = sizeof(NodeType) * sharedState->nodeList.size();

            // extend with sizeof node depending on node type
            for (int i = 0; i < sharedState->nodeList.size(); i++)
            {

                if ( sharedState->nodeTypeList[i] == NodeType::GEO)
                    responseLength += sizeof_nodegeo;
                else if ( sharedState->nodeTypeList[i] == NodeType::LIGHT)
                    responseLength += sizeof_nodelight;
                else if ( sharedState->nodeTypeList[i] == NodeType::CAMERA)
                    responseLength += sizeof_nodecam;
                else
                    responseLength += sizeof_node;

                /*
                Node* node = sharedState->nodeList[i];
                if (dynamic_cast<NodeGeo*>( node ))
                    responseLength += sizeof_nodegeo;
                else if (dynamic_cast<NodeLight*>(node))
                    responseLength += sizeof_nodelight;
                else if (dynamic_cast<NodeCam*>(node))
                    responseLength += sizeof_nodecam;
                else
                    responseLength += sizeof_node;

                */
            }




            // allocate memory for out byte stream
            messageStart = responseMessageContent = (char*)malloc(responseLength);


            // iterate over node list copy data to out byte stream
            for (int i = 0; i < sharedState->nodeList.size(); i++)
            {
                Node* node = sharedState->nodeList[i];


                // First Copy node type
                int nodeType = sharedState->nodeTypeList[i];
                memcpy(responseMessageContent, (char*)&nodeType, sizeof(int));
                responseMessageContent += sizeof(int);

                // Copy specific node data
                if (sharedState->nodeTypeList[i] == NodeType::GEO)
                {
                    memcpy(responseMessageContent, node, sizeof_nodegeo);
                    responseMessageContent += sizeof_nodegeo;
                }
                else if (sharedState->nodeTypeList[i] == NodeType::LIGHT)
                {
                    memcpy(responseMessageContent, node, sizeof_nodelight);
                    responseMessageContent += sizeof_nodelight;
                }
                else if (sharedState->nodeTypeList[i] == NodeType::CAMERA)
                {
                    memcpy(responseMessageContent, node, sizeof_nodecam);
                    responseMessageContent += sizeof_nodecam;
                }
                else
                {
                    memcpy(responseMessageContent, node, sizeof_node);
                    responseMessageContent += sizeof_node;
                }

                /*
                Node* node = sharedState->nodeList[i];
                NodeGeo* nodeGeo = dynamic_cast<NodeGeo*>(node);
                NodeLight* nodeLight = dynamic_cast<NodeLight*>(node);
                NodeCam* nodeCam = dynamic_cast<NodeCam*>(node);

                // First Copy node type
                int nodeType = NodeType::GROUP;

                if ( nodeGeo )
                    nodeType = NodeType::GEO;
                else if ( nodeLight )
                    nodeType = NodeType::LIGHT;
                else if ( nodeCam )
                    nodeType = NodeType::CAMERA;


                memcpy(responseMessageContent, (char*)&nodeType, sizeof(int));
                responseMessageContent += sizeof(int);

                // Copy specific node data
                if (nodeGeo)
                {
                    memcpy(responseMessageContent, &nodeGeo, sizeof_nodegeo);
                    responseMessageContent += sizeof_nodegeo;
                }
                else if (nodeLight)
                {
                    memcpy(responseMessageContent, &nodeLight, sizeof_nodelight);
                    responseMessageContent += sizeof_nodelight;
                }
                else if (nodeCam)
                {
                    memcpy(responseMessageContent, &nodeCam, sizeof_nodecam);
                    responseMessageContent += sizeof_nodecam;
                }
                else
                {
                    memcpy(responseMessageContent, &node, sizeof_node);
                    responseMessageContent += sizeof_node;
                }
                */

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
#if KATANA_VERSION_MAJOR >= 3

	// The render action used for this render output:
	// Set the render action to do nothing:
	FnKat::Render::DiskRenderOutputProcess::RenderActionPtr renderAction(
		new FnKat::Render::NoOutputRenderAction());

	// Pass ownership of the renderAction to the diskRenderOutputProcess:
	diskRenderOutputProcess.setRenderAction(
		FnKat::Render::DiskRenderOutputProcess::UniquePtr::move(renderAction));

// OLD KATANA 2.x IMPLEMENTATION
#else
    // The render action used for this render output:
    //std::auto_ptr<FnKat::Render::RenderAction> renderAction;

    //// Set the render action to do nothing:
    //renderAction.reset(new FnKat::Render::NoOutputRenderAction());

    //// Pass ownership of the renderAction to the diskRenderOutputProcess:
    //diskRenderOutputProcess.setRenderAction(renderAction);
#endif

}

DEFINE_RENDER_PLUGIN(SceneDistributorPlugin)

}
}

void registerPlugins()
{
    REGISTER_PLUGIN(Dreamspace::Katana::SceneDistributorPlugin, "sceneDistributor", 0, 1);
}

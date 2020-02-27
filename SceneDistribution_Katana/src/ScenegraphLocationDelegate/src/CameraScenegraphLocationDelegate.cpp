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
#include "PluginState.h"
#include "CameraScenegraphLocationDelegate.h"

#include <FnRenderOutputUtils/FnRenderOutputUtils.h>

#include <glm/gtx/string_cast.hpp>

#include <string>


CameraScenegraphLocationDelegate* CameraScenegraphLocationDelegate::create()
{
    return new CameraScenegraphLocationDelegate();
}

void CameraScenegraphLocationDelegate::flush()
{
}

std::string CameraScenegraphLocationDelegate::getSupportedRenderer() const
{
    return std::string("sceneDistributor");
}

void CameraScenegraphLocationDelegate::fillSupportedLocationList(std::vector<std::string>& supportedLocationList) const
{
    supportedLocationList.push_back(std::string("camera"));
}

float CameraScenegraphLocationDelegate::hFovToVFov(float hFov, float width/* = 16.0f*/, float height/* = 9.0f*/)
{
	return glm::degrees(2 * glm::atan(glm::tan(glm::radians(hFov) / 2.0f) * (height / width)));
}

void* CameraScenegraphLocationDelegate::process(FnKat::FnScenegraphIterator sgIterator, void* optionalInput)
{

    // get state
    Dreamspace::Katana::SceneDistributorPluginState* sharedState = reinterpret_cast<Dreamspace::Katana::SceneDistributorPluginState*>(optionalInput);

    // create camera node
    Dreamspace::Katana::NodeCam* nodeCam =  new Dreamspace::Katana::NodeCam();

    sharedState->nodeTypeList.push_back(Dreamspace::Katana::NodeType::CAMERA);


    FnAttribute::GroupAttribute attributesGroup = sgIterator.getAttribute("geometry");

    if ( attributesGroup.isValid() )
    {
        // std::cout << attributesGroup.getXML() << std::endl;
        // Fov
        FnAttribute::DoubleAttribute floatAttr = attributesGroup.getChildByName("fov");
        if ( floatAttr.isValid() )
        {
            //nodeCam->fov = hFovToVFov(floatAttr.getValue(70, false));
            nodeCam->fov = floatAttr.getValue(70, false);
        }
        // Near
        floatAttr = attributesGroup.getChildByName("near");
        if ( floatAttr.isValid() )
        {
            nodeCam->near = floatAttr.getValue(0.1, false);
        }
        // Far
        floatAttr = attributesGroup.getChildByName("far");
        if ( floatAttr.isValid() )
        {
            nodeCam->far = floatAttr.getValue(1000, false);
        }
    }
    else
    {
        std::cout << "[INFO SceneDistributor.CameraScenegraphLocationDelegate] Camera attribute group not found. " << std::endl;
    }


    std::cout << "[INFO SceneDistributor.CameraScenegraphLocationDelegate] Camera FOV: " << nodeCam->fov << " Near: "  << nodeCam->near << " Far: "  << nodeCam->far << std::endl;

    // store at sharedState to access it in iterator
    sharedState->node = nodeCam;
    sharedState->numCameras++;

    return NULL;
}

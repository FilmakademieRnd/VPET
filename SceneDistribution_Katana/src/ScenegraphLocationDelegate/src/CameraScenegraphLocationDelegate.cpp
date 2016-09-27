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

void* CameraScenegraphLocationDelegate::process(FnKat::FnScenegraphIterator sgIterator, void* optionalInput)
{

    // get state
    Dreamspace::Katana::SceneDistributorPluginState* sharedState = reinterpret_cast<Dreamspace::Katana::SceneDistributorPluginState*>(optionalInput);

    // create camera node
    Dreamspace::Katana::NodeCam* nodeCam =  new Dreamspace::Katana::NodeCam();


    FnAttribute::GroupAttribute attributesGroup = sgIterator.getAttribute("geometry");

    if ( attributesGroup.isValid() )
    {
        // std::cout << attributesGroup.getXML() << std::endl;
        // Fov
        FnAttribute::DoubleAttribute floatAttr = attributesGroup.getChildByName("fov");
        if ( floatAttr.isValid() )
        {
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

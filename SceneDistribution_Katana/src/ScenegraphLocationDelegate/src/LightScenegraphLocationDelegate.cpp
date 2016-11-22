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
#include "PluginState.h"
#include "LightScenegraphLocationDelegate.h"

#include <FnRenderOutputUtils/FnRenderOutputUtils.h>

#include <glm/gtx/string_cast.hpp>

#include <string>


LightScenegraphLocationDelegate* LightScenegraphLocationDelegate::create()
{
    return new LightScenegraphLocationDelegate();
}

void LightScenegraphLocationDelegate::flush()
{
}

std::string LightScenegraphLocationDelegate::getSupportedRenderer() const
{
    return std::string("sceneDistributor");
}

void LightScenegraphLocationDelegate::fillSupportedLocationList(std::vector<std::string>& supportedLocationList) const
{
    supportedLocationList.push_back(std::string("light"));
}

void* LightScenegraphLocationDelegate::process(FnKat::FnScenegraphIterator sgIterator, void* optionalInput)
{
    // get state
    Dreamspace::Katana::SceneDistributorPluginState* sharedState = reinterpret_cast<Dreamspace::Katana::SceneDistributorPluginState*>(optionalInput);

    // create light node
    Dreamspace::Katana::NodeLight* nodeLight =  new Dreamspace::Katana::NodeLight();


    // std::cout << "[INFO SceneDistributor.LightScenegraphLocationDelegate] Processing location: " << location << std::endl;

    // Light material
    FnAttribute::GroupAttribute materialAttr = FnKat::RenderOutputUtils::getFlattenedMaterialAttr(sgIterator, sharedState->materialTerminalNamesAttr);

    // std::cout << "[INFO SceneDistributor.LightScenegraphLocationDelegate] Material:" << std::endl;
    // std::cout << materialAttr.getXML() << std::endl;

    // Light attributes
    FnAttribute::GroupAttribute paramsAttr = materialAttr.getChildByName("parameters");
    if ( paramsAttr.isValid() )
    {

          // std::cout << paramsAttr.getXML() << std::endl;

          FnAttribute::StringAttribute  lightTypeAttr = paramsAttr.getChildByName("type");
          std::string lightAreaType = lightTypeAttr.getValue("<default>", false);

          if ( lightAreaType == "sphere" )
          {
              nodeLight->type = Dreamspace::Katana::POINT;
          }
          else if ( lightAreaType == "directional")
          {
              nodeLight->type = Dreamspace::Katana::DIRECTIONAL;
          }
          else if ( lightAreaType == "disk")
          {
              nodeLight->type = Dreamspace::Katana::SPOT;

              // Angle
              FnAttribute::FloatAttribute angleAttr = paramsAttr.getChildByName("coneAngle");
              if ( angleAttr.isValid() )
              {
                  nodeLight->angle = angleAttr.getValue( 120.0, false );
              }

          }
          else if ( lightAreaType == "env")
          {
              // TODO: handle
              nodeLight->type = Dreamspace::Katana::NONE;
          }
          else //rect
          {
              nodeLight->type = Dreamspace::Katana::SPOT;
              nodeLight->angle = 180.0;
          }

          // Color
          FnAttribute::FloatAttribute colorAttr = paramsAttr.getChildByName("color");
          if ( colorAttr.isValid() )
          {
              // Get the color value
              FnAttribute::FloatConstVector colorData = colorAttr.getNearestSample(0.0f);

              nodeLight->color[0] = colorData[0];
              nodeLight->color[1] = colorData[1];
              nodeLight->color[2] = colorData[2];
          }

          // Intensity
          FnAttribute::FloatAttribute intensityAttr = paramsAttr.getChildByName("intensity");
          if ( intensityAttr.isValid() )
          {
              nodeLight->intensity = intensityAttr.getValue( 1.0, false );
          }

          // Exposure
          FnAttribute::FloatAttribute exposureAttr = paramsAttr.getChildByName("exposure");
          if ( exposureAttr.isValid() )
          {
              nodeLight->exposure = exposureAttr.getValue( 3.0, false );
          }

          std::cout << "[INFO SceneDistributor.LightScenegraphLocationDelegate] Light color: " << nodeLight->color[0] << " "  << nodeLight->color[1] << " "  << nodeLight->color[2] << " Type: " << lightAreaType << " intensity: " << nodeLight->intensity  << " exposure: " << nodeLight->exposure  << " coneAngle: " << nodeLight->angle << std::endl;

    }
    else
    {
        std::cout << "[INFO SceneDistributor.LightScenegraphLocationDelegate] Common Parameters not found. " << std::endl;
    }

    if ( nodeLight->type != Dreamspace::Katana::NONE )
    {
        // store at sharedState to access it in iterator
        sharedState->node = nodeLight;
        sharedState->numLights++;
    }
    else
    {
        delete nodeLight;
        Dreamspace::Katana::Node* node = new Dreamspace::Katana::Node();
        sharedState->node = node;
        std::cout << "[INFO SceneDistributor.LightScenegraphLocationDelegate] Found unknown Light (add as group)" << std::endl;
    }

    return NULL;

}


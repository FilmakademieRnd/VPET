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

#include "SceneIterator.h"

#include <FnRenderOutputUtils/FnRenderOutputUtils.h>
#include <FnGeolibServices/FnXFormUtil.h>


#include <string>

#include <vector>

#include <glm/vec3.hpp>
#include <glm/gtc/quaternion.hpp>

#include <glm/gtc/matrix_transform.hpp>
#include <glm/gtc/matrix_inverse.hpp>
#include <glm/gtx/matrix_decompose.hpp>
#include <glm/gtx/string_cast.hpp>


namespace Dreamspace
{
namespace Katana
{
namespace SceneIterator
{

void buildLocation(FnKat::FnScenegraphIterator sgIterator, SceneDistributorPluginState* sharedState)
{

    // Check Tag and return if not equal
    if ( sharedState->lodMode == TAG )
    {
        FnAttribute::StringAttribute lodTagAttr = sgIterator.getAttribute("info.componentLodTag");
        if ( lodTagAttr.isValid() )
        {
            if ( sharedState->lodTag != lodTagAttr.getValue( "unknown", false ) )
            {
                return;
            }
        }
    }

    std::cout << "Build: " <<  sgIterator.getFullName() << std::endl;

    // Get scenegraph location type
    std::string type = sgIterator.getType();

    // Get transform at this location
    glm::mat4 transform = glm::mat4(1.0f);

    /*
    std::vector<FnKat::RenderOutputUtils::Transform> xFormList;
    FnKat::RenderOutputUtils::fillXFormListForLocation(xFormList, sgIterator);

    glm::mat4 singleScale = glm::mat4(1.0);
    glm::mat4 singleRotate = glm::mat4(1.0);
    glm::mat4 singleTranslate = glm::mat4(1.0);
    glm::mat4 singleMatrix = glm::mat4(1.0);
    */

    FnAttribute::GroupAttribute xformAttr = sgIterator.getAttribute("xform");

    if (xformAttr.isValid())
    {
      FnAttribute::DoubleAttribute matrixAttr = FnGeolibServices::FnXFormUtil::CalcTransformMatrixAtTime(xformAttr, 0.0f).first;
      FnAttribute::DoubleConstVector matrixData = matrixAttr.getNearestSample(0.0);
//      transform = glm::mat4(matrixData[ 0], matrixData[ 1], matrixData[ 2], matrixData[ 3],
//                       matrixData[ 4], matrixData[ 5], matrixData[ 6], matrixData[ 7],
//                       matrixData[ 8], matrixData[ 9], matrixData[10], matrixData[11],
//                       matrixData[12], matrixData[13], matrixData[14], matrixData[15]);

      // TODO: Hardcoded handiness
      if ( sharedState->nodeTypeList[sharedState->nodeTypeList.size()-1] == Dreamspace::Katana::NodeType::CAMERA || sharedState->nodeTypeList[sharedState->nodeTypeList.size()-1] == Dreamspace::Katana::NodeType::LIGHT)
      {

          transform = glm::mat4(matrixData[ 0], -matrixData[ 4], -matrixData[ 8], matrixData[ 3],
                           -matrixData[ 1], matrixData[ 5], matrixData[ 9], matrixData[ 7],
                           -matrixData[ 2], matrixData[ 6], matrixData[10], matrixData[11],
                           -matrixData[12], matrixData[13], matrixData[14], matrixData[15]);
      }
      else
      {
          transform = glm::mat4(matrixData[ 0], -matrixData[ 4], -matrixData[ 8], matrixData[ 3],
                           -matrixData[ 1], matrixData[ 5], matrixData[ 9], matrixData[ 7],
                           -matrixData[ 2], matrixData[ 6], matrixData[10], matrixData[11],
                           -matrixData[12], matrixData[13], matrixData[14], matrixData[15]);

      }

    }




    // Build scene graps >>
    sharedState->node = NULL;

    // const std::string &nodeType = sgIterator.getType();


    bool pluginFound = FnKat::RenderOutputUtils::processLocation(sgIterator, "sceneDistributor", type, sharedState, NULL);

    if ( !pluginFound )
    {
        std::cout << "NO processLocation for " << sgIterator.getType() << std::endl;
        sharedState->node = new Node();
        sharedState->nodeTypeList.push_back(NodeType::GROUP);
    }

    // node name
    std::string name_str = sgIterator.getName();
    name_str = name_str.substr(0, 63);
    strcpy(sharedState->node->name, name_str.c_str());

    sharedState->node->childCount = 0;

    for (FnKat::FnScenegraphIterator child = sgIterator.getFirstChild(); child.isValid(); child = child.getNextSibling())
    {
        if ( sharedState->lodMode == TAG )
        {
            FnAttribute::StringAttribute lodTagAttr = child.getAttribute("info.componentLodTag");
            if ( lodTagAttr.isValid() )
            {
                if ( sharedState->lodTag == lodTagAttr.getValue( "unknown", false ) )
                {
                    sharedState->node->childCount++;
                }
            }
            else
            {
                sharedState->node->childCount++;
            }
        }
        else
        {
            sharedState->node->childCount++;
        }

    }





    glm::vec3 skew, position, scale;
    glm::vec4 persp;
    glm::quat _rotation;
    glm::quat rotation;
    glm::decompose(transform, scale, _rotation, position, skew, persp);

    if ( sharedState->nodeTypeList[sharedState->nodeTypeList.size()-1] == Dreamspace::Katana::NodeType::CAMERA || sharedState->nodeTypeList[sharedState->nodeTypeList.size()-1] == Dreamspace::Katana::NodeType::LIGHT)
    {
        glm::vec3 axis (0, 1, 0);
        float angle = 3.14;
        rotation = glm::rotate(_rotation, angle, axis);
    }
    else
    {
        rotation = _rotation;
    }

    //std::cout << "pos: " << position[0] << " " << position[1] << " " << position[2] << std::endl;
    // std::cout << "rot: " << rotation[0] << " " << rotation[1] << " " << rotation[2] << " " << rotation[3] << std::endl;

    // TODO: Hardcoded handiness
    sharedState->node->position[0] = position[0];
    sharedState->node->position[1] = position[1];
    sharedState->node->position[2] = position[2];

    sharedState->node->rotation[0] = rotation[0];
    sharedState->node->rotation[1] = rotation[1];
    sharedState->node->rotation[2] = rotation[2];
    sharedState->node->rotation[3] = rotation[3];


    sharedState->node->scale[0] = scale[0];
    sharedState->node->scale[1] = scale[1];
    sharedState->node->scale[2] = scale[2];





    sharedState->node->editable=false;

    FnAttribute::IntAttribute editAttr = sgIterator.getAttribute("dreamspace.editable");
    if ( editAttr.isValid() )
    {
        if ( 1 == editAttr.getValue( 0, false ) )
        {
            sharedState->node->editable=true;
        }
    }

    std::cout << "[INFO SceneDistributor.SceneIterator] Add Node: " << sharedState->node->name << std::endl;
    sharedState->nodeList.push_back(sharedState->node);


	//std::string fullname = sgIterator.getName();
    //for ( int i =0; i<fullname.size(); i++ )
    //{
    //    node->name[i]  = fullname[i];
    //}
    // node->name.resize(40);
    
    // << Build scene graps


    //sharedState->modelTransform = transform;

    // Stack pre-multiplied model matrix
    //sharedState->modelMatrices.push(sharedState->modelMatrices.top() * transform);

    // Delegate processing of this location type by plugins defined in ScenegraphLocationDelegate
    //bool pluginFound = FnKat::RenderOutputUtils::processLocation(sgIterator, "sceneDistributor", type, sharedState, NULL);


    // Recurse to children
    for (FnKat::FnScenegraphIterator child = sgIterator.getFirstChild(); child.isValid(); child = child.getNextSibling())
    {
        buildLocation(child, sharedState);
    }

    // Unstack pre-multiplied model matrix before returning
    //sharedState->modelMatrices.pop();
}

}
}
}

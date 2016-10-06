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

#include "SceneIterator.h"

#include <FnRenderOutputUtils/FnRenderOutputUtils.h>
#include <FnGeolibServices/FnXFormUtil.h>


#include <string>
#include <vector>

#include <glm/vec3.hpp>

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
      transform = glm::mat4(matrixData[ 0], matrixData[ 1], matrixData[ 2], matrixData[ 3],
                       matrixData[ 4], matrixData[ 5], matrixData[ 6], matrixData[ 7],
                       matrixData[ 8], matrixData[ 9], matrixData[10], matrixData[11],
                       matrixData[12], matrixData[13], matrixData[14], matrixData[15]);
    }


    /*

    // std::cout<< " xFormList size (transforms at sgIterator) " << xFormList.size() << std::endl;

    for (std::vector<FnKat::RenderOutputUtils::Transform>::const_iterator xFormIt = xFormList.begin(); xFormIt != xFormList.end(); ++xFormIt)
    {

        FnKat::RenderOutputUtils::TransformList transformList = xFormIt->transformList;

        // std::cout<< " transformList size (single transform ) " << transformList.size() << std::endl;

        for (FnKat::RenderOutputUtils::TransformList::const_iterator transformIt = transformList.begin(); transformIt != transformList.end(); ++transformIt)
        {
            std::string transformType = transformIt->first;
            FnKat::RenderOutputUtils::TransformData transformData = transformIt->second;

            if (transformType == "translate" && transformData.size() == 3)
            {
                singleTranslate *= glm::translate( glm::mat4(1.0), glm::vec3(-transformData[0], -transformData[1], -transformData[2]));
                // std::cout << "Translate " << glm::to_string(singleTranslate) << std::endl;

            }
            else if (transformType == "rotate" && transformData.size() == 4)
            {
                glm::vec3 axis(transformData[1], transformData[2], transformData[3]);
                singleRotate *= glm::rotate( glm::mat4(1.0), -transformData[0] * 0.01745329252f, axis);
                //std::cout << "rotate " << glm::to_string(singleRotate) << std::endl;
            }
            else if (transformType == "scale" && transformData.size() == 3)
            {
                glm::vec3 scale(1.0f / transformData[0], 1.0f / transformData[1], 1.0f / transformData[2]);
                singleScale *= glm::scale( glm::mat4(1.0), scale);
                //std::cout << "Scale " << glm::to_string(singleScale) << std::endl;
            }
            else if (transformType == "matrix" && transformData.size() == 16)
            {
                glm::mat4 matrix(transformData[0], transformData[1], transformData[2], transformData[3],
                                 transformData[ 4], transformData[ 5], transformData[ 6], transformData[ 7],
                                 transformData[ 8], transformData[ 9], transformData[10], transformData[11],
                                 transformData[12], transformData[13], transformData[14], transformData[15]);
                singleMatrix = glm::affineInverse(matrix);
                // std::cout << "singleMatrix " << glm::to_string(singleMatrix) << std::endl;
            }
        }

        /*
        FnKat::RenderOutputUtils::TransformList transformList = xFormIt->transformList;
        for (FnKat::RenderOutputUtils::TransformList::const_iterator transformIt = transformList.begin(); transformIt != transformList.end(); ++transformIt)
        {
            std::string transformType = transformIt->first;
            FnKat::RenderOutputUtils::TransformData transformData = transformIt->second;

			glm::mat4 singleTransform = glm::mat4(1.0f);
            if (transformType == "translate" && transformData.size() == 3)
            {
				singleTransform = glm::translate(singleTransform, glm::vec3(-transformData[0], -transformData[1], -transformData[2]));
            }
            else if (transformType == "rotate" && transformData.size() == 4)
            {
                glm::vec3 axis(transformData[1], transformData[2], transformData[3]);
				glm::rotate(singleTransform, -transformData[0] * 0.01745329252f, axis);
            }
            else if (transformType == "scale" && transformData.size() == 3)
            {
				glm::vec3 scale(1.0f / transformData[0], 1.0f / transformData[1], 1.0f / transformData[2]);
                glm::scale(singleTransform,scale);
            }
            else if (transformType == "matrix" && transformData.size() == 16)
            {
				glm::mat4 matrix(transformData[0], transformData[1], transformData[2], transformData[3],
                                 transformData[ 4], transformData[ 5], transformData[ 6], transformData[ 7],
                                 transformData[ 8], transformData[ 9], transformData[10], transformData[11],
                                 transformData[12], transformData[13], transformData[14], transformData[15]);
                singleTransform = glm::affineInverse(matrix);
            }
            transform *= singleTransform;

         }
         *
    }

    // combine it
    transform = (singleTranslate * singleRotate * singleScale);
    */


    // Build scene graps >>
    sharedState->node = NULL;

    // const std::string &nodeType = sgIterator.getType();


    bool pluginFound = FnKat::RenderOutputUtils::processLocation(sgIterator, "sceneDistributor", type, sharedState, NULL);

    if ( !pluginFound )
    {
        std::cout << "NO processLocation for " << sgIterator.getType() << std::endl;
        sharedState->node = new Node();
    }

    sharedState->node->name = sgIterator.getName();
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
    glm::quat rotation;
    glm::decompose(transform, scale, rotation, position, skew, persp);


    //std::cout << "pos: " << position[0] << " " << position[1] << " " << position[2] << std::endl;
    std::cout << "pos: " << rotation[0] << " " << rotation[1] << " " << rotation[2] << " " << rotation[3] << std::endl;

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

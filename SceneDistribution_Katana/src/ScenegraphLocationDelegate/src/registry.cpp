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
#include "GeometryScenegraphLocationDelegate.h"
#include "LightScenegraphLocationDelegate.h"
#include "CameraScenegraphLocationDelegate.h"

DEFINE_SCENEGRAPH_LOCATION_DELEGATE_PLUGIN(GeometryScenegraphLocationDelegate)
DEFINE_SCENEGRAPH_LOCATION_DELEGATE_PLUGIN(LightScenegraphLocationDelegate)
DEFINE_SCENEGRAPH_LOCATION_DELEGATE_PLUGIN(CameraScenegraphLocationDelegate)

void registerPlugins()
{
    REGISTER_PLUGIN(GeometryScenegraphLocationDelegate, "sceneDistributorGeometryScenegraphLocationDelegate", 0, 1);
    REGISTER_PLUGIN(LightScenegraphLocationDelegate, "sceneDistributorLightScenegraphLocationDelegate", 0, 1);
    REGISTER_PLUGIN(CameraScenegraphLocationDelegate, "sceneDistributorCameraScenegraphLocationDelegate", 0, 1);
}

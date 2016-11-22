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
#ifndef COMPANYNAMEKATANA_SCENEITERATOR_H
#define COMPANYNAMEKATANA_SCENEITERATOR_H

#include "PluginState.h"

#include <FnScenegraphIterator/FnScenegraphIterator.h>


namespace Dreamspace
{
namespace Katana
{
namespace SceneIterator
{
    void buildLocation(FnKat::FnScenegraphIterator sgIterator, SceneDistributorPluginState* sharedState);
}
}
}

#endif

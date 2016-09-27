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
#ifndef GEOMETRYSCENEGRAPHLOCATIONDELEGATE_H
#define GEOMETRYSCENEGRAPHLOCATIONDELEGATE_H

#include <FnRender/plugin/ScenegraphLocationDelegate.h>

#include "PluginState.h"

class GeometryScenegraphLocationDelegate : public Foundry::Katana::Render::ScenegraphLocationDelegate
{
public:
    static GeometryScenegraphLocationDelegate* create();
    static void flush();

    virtual std::string getSupportedRenderer() const;
    virtual void fillSupportedLocationList(std::vector<std::string>& supportedLocationList) const;
    FnAttribute::Attribute GetAttribute( FnAttribute::GroupAttribute i_attr, std::string i_name );
    bool LoadMap( std::string i_filepath, unsigned char* &o_buffer,  int* o_bufferSize  );
    virtual void* process(FnKat::FnScenegraphIterator sgIterator, void* optionalInput);

private:

    inline bool search_path_textpack( const std::string &key, const Dreamspace::Katana::TexturePackage &texPack) { return ( texPack.path == key ); }


};

#endif


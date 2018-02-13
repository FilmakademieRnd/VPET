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

#ifndef COMPANYNAMEKATANA_SCENEDISTRIBUTORINFOPLUGIN_H
#define COMPANYNAMEKATANA_SCENEDISTRIBUTORINFOPLUGIN_H

#include "PluginState.h"

#include <FnRender/plugin/RenderBase.h>

#include <string>
#include <queue>

#include <zmq.hpp>

#include <pthread.h>


namespace Dreamspace
{
namespace Katana
{

    /**
     * \ingroup SceneDistributorPlugin
     */

    /**
     * @brief SceneDistributor Render Plugin
     */
    class SceneDistributorPlugin : public FnKat::Render::RenderBase
    {
    public:

        SceneDistributorPlugin(FnKat::FnScenegraphIterator rootIterator,
                             FnKat::GroupAttribute arguments);
        ~SceneDistributorPlugin();

        // Render Control

        int start();

        int pause();

        int resume();

        int stop();

        // Interactive live updates

        int startLiveEditing();

        int stopLiveEditing();

        int processControlCommand(const std::string& command);

        int queueDataUpdates(FnKat::GroupAttribute updateAttribute);

        int applyPendingDataUpdates();

        bool hasPendingDataUpdates() const;

        // Disk Render

        void configureDiskRenderOutputProcess(FnKat::Render::DiskRenderOutputProcess& diskRenderOutputProcess,
                                              const std::string& outputName,
                                              const std::string& outputPath,
                                              const std::string& renderMethodName,
                                              const float& frameTime) const;

        // Plugin Interface

        static Foundry::Katana::Render::RenderBase* create(FnKat::FnScenegraphIterator rootIterator, FnKat::GroupAttribute args)
        {
            return new SceneDistributorPlugin(rootIterator, args);
        }


        static void flush()
        {

        }

    private:
        SceneDistributorPluginState _sharedState;

        // Live render
        struct Update
        {
            std::string type;
            std::string location;
            FnAttribute::GroupAttribute attributesAttr;
            FnAttribute::GroupAttribute xformAttr;
        };
        typedef std::queue<Update> UpdateQueue;

        UpdateQueue _cameraUpdates;
        void updateCamera();

		//zeroMQ context

		//zeroMQ thread
		pthread_t thread;
		
	
    };
    
    static void* server(void* scene);

    /**
     * @}
     */
}
}

#endif

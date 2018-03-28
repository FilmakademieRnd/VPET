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

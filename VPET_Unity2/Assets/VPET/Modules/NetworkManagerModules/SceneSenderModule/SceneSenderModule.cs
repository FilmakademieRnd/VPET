/*
-------------------------------------------------------------------------------
VPET - Virtual Production Editing Tools
vpet.research.animationsinstitut.de
https://github.com/FilmakademieRnd/VPET
 
Copyright (c) 2021 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab
 
This project has been initiated in the scope of the EU funded project 
Dreamspace (http://dreamspaceproject.eu/) under grant agreement no 610005 2014-2016.
 
Post Dreamspace the project has been further developed on behalf of the 
research and development activities of Animationsinstitut.
 
In 2018 some features (Character Animation Interface and USD support) were
addressed in the scope of the EU funded project  SAUCE (https://www.sauceproject.eu/) 
under grant agreement no 780470, 2018-2020
 
VPET consists of 3 core components: VPET Unity Client, Scene Distribution and
Syncronisation Server. They are licensed under the following terms:
-------------------------------------------------------------------------------
*/

//! @file "SceneDataHandler.cs"
//! @brief Implementation of the scene sender module, listening to scene requests and sending scene data. 
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 20.05.2021

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vpet
{
    //!
    //! Class implementing the scene sender module, listening to scene requests and sending scene data.
    //!
    public class SceneSenderModule : NetworkManagerModule
    {
        //!
        //! Constructor.
        //! Initialisation of all members and setup of the response messages.
        //!
        public SceneSenderModule(string name) : base(name)
        {
            name = base.name;

         
        }

        //!
        //! Function to start the scene sender module.
        //! @param ip The IP address to be used from the sender.
        //! @param port The port number to be used from the sender.
        //!
        public void sendScene(string ip, string port)
        {
            Dictionary<string, byte[]> responses = new Dictionary<string, byte[]>();
            SceneManager.SceneDataHandler dataHandler = ((SceneManager)manager.core.getManager(typeof(SceneManager))).sceneDataHandler;

            responses.Add("header", dataHandler.headerByteData);
            responses.Add("nodes", dataHandler.nodesByteData);
            responses.Add("objects", dataHandler.objectsByteData);
            responses.Add("characters", dataHandler.characterByteData);
            responses.Add("textures", dataHandler.texturesByteData);
            responses.Add("materials", dataHandler.materialsByteData);

            manager.startResponder(ip, port, ref responses);
        }

        //!
        //! Function to stop the scene sender module.
        //!
        public void stop()
        {
            manager.stopResponder();
        }

    }
}

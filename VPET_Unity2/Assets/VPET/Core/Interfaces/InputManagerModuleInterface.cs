/*
-------------------------------------------------------------------------------
VPET - Virtual Production Editing Tools
vpet.research.animationsinstitut.de
https://github.com/FilmakademieRnd/VPET
 
Copyright (c) 2022 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab
 
This project has been initiated in the scope of the EU funded project 
Dreamspace (http://dreamspaceproject.eu/) under grant agreement no 610005 2014-2016.
 
Post Dreamspace the project has been further developed on behalf of the 
research and development activities of Animationsinstitut.
 
In 2018 some features (Character Animation Interface and USD support) were
addressed in the scope of the EU funded project  SAUCE (https://www.sauceproject.eu/) 
under grant agreement no 780470, 2018-2022
 
VPET consists of 3 core components: VPET Unity Client, Scene Distribution and
Syncronisation Server. They are licensed under the following terms:
-------------------------------------------------------------------------------
*/

//! @file "InputManagerModuleInterface.cs"
//! @brief Implementation of the input manager module interface.
//! @author Simon Spielmann
//! @author Jonas Trottnow
//! @version 0
//! @date 23.06.2021

namespace vpet
{
    public class InputManagerModule : Module
    {
        //!
        //! constructor
        //! @param  name The name of the module.
        //!
        public InputManagerModule(string name, Core core) : base(name, core) { }
    }
}
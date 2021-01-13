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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace vpet
{
    public class AnimSender : MonoBehaviour
    {
        private ServerAdapter serverAdapter;
        private SceneObject ownSceneObject;

        // Start is called before the first frame update
        void Start()
        {
            serverAdapter = GameObject.Find("ServerAdapter").GetComponent<ServerAdapter>();
            InvokeRepeating("sendAnimation", 0.0f, 0.04f);
        }

        void sendAnimation()
        {
            if (!ownSceneObject)
            {
                ownSceneObject = this.GetComponent<SceneObject>();
                //ownSceneObject.locked = true;
            }
            serverAdapter.SendObjectUpdate(ownSceneObject,ParameterType.BONEANIM);
        }
    }
}

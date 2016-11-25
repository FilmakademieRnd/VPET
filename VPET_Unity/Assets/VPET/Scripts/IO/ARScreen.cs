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
#if USE_TANGO
using Tango;
using UnityEngine;
using UnityEngine.Rendering;

namespace vpet
{

    public class ARScreen : TangoARScreen
    {

        private bool firstActivation = true;
        private CommandBuffer buf = null;

        // Use this for initialization
        void OnEnable()
        {
            if ( firstActivation )
            {
                print("firstActivation");
                firstActivation = false;
            }
            else
            {
                print("Not firstActivation");
                // assign command buffer
                Camera camera = transform.GetComponent<Camera>();
                buf = VideoOverlayProvider.CreateARScreenCommandBuffer();
                camera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, buf);
                camera.AddCommandBuffer(CameraEvent.BeforeGBuffer, buf);

            }
        }

        // Update is called once per frame
        void OnDisable()
        {
            Camera camera = transform.GetComponent<Camera>();
            if ( buf == null && camera.commandBufferCount > 0 )
            {
                buf = camera.GetCommandBuffers(CameraEvent.BeforeForwardOpaque)[camera.commandBufferCount - 1];
            }
            camera.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, buf);
            camera.RemoveCommandBuffer(CameraEvent.BeforeGBuffer, buf);
        }
    }
}
#endif
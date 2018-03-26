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
                firstActivation = false;
            }
            else
            {
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

#elif USE_ARKIT
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.iOS;

namespace vpet
{
	public class ARScreen : UnityARVideo
	{
		void Awake()
		{
			if (m_ClearMaterial == null)
				m_ClearMaterial = Resources.Load ("VPET/Materials/YUVMaterial", typeof(Material)) as Material;
		}
	}
}
#endif
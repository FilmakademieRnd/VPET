/*
VPET - Virtual Production Editing Tools
tracer.research.animationsinstitut.de
https://github.com/FilmakademieRnd/VPET

Copyright (c) 2023 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

This project has been initiated in the scope of the EU funded project
Dreamspace (http://dreamspaceproject.eu/) under grant agreement no 610005 2014-2016.

Post Dreamspace the project has been further developed on behalf of the
research and development activities of Animationsinstitut.

In 2018 some features (Character Animation Interface and USD support) were
addressed in the scope of the EU funded project SAUCE (https://www.sauceproject.eu/)
under grant agreement no 780470, 2018-2022

This program is free software; you can redistribute it and/or modify it under
the terms of the MIT License as published by the Open Source Initiative.

This program is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE. See the MIT License for more details.

You should have received a copy of the MIT License along with
this program; if not go to
https://opensource.org/licenses/MIT
*/


//! @file "kill.cs"
//! @brief scanning a QR code  
//! @author Alexandru Schwartz
//! @version 0
//! @date 26.10.2023

using System.Collections;
using System.Collections.Generic;
using System;
using System.Text.RegularExpressions;
using tracer;
using UnityEngine;
using UnityEngine.UI;
using ZXing;

namespace tracer
{
    public class QRScanner : MonoBehaviour
    {
        //!
        //! Variables for handling webcam and QR code
        //!
        WebCamTexture webcamTexture;
        string QrCode = string.Empty;

        //!
        //! References to the Core and SceneReceiverModule
        //!
        private Core _core;
        private SceneReceiverModule _sceneRec;

        //!
        //! Initialization method
        //!
        void Start()
        {
            // Initialize references to Core and SceneReceiverModule
            _core = GameObject.FindGameObjectWithTag("Core").GetComponent<Core>();
            _sceneRec = _core.getManager<NetworkManager>().getModule<SceneReceiverModule>();
            
            // Get the RawImage component for rendering the webcam feed
            var renderer = GetComponent<RawImage>();
            webcamTexture = new WebCamTexture(500, 500); // Initialize the webcam texture
            renderer.texture = webcamTexture; // Assign the webcam texture to the RawImage component
            
            // Start the QR code scanning process
            StartCoroutine(GetQRCode());
        }

        //!
        //! Coroutine for continuously capturing and decoding QR codes
        //!
        IEnumerator GetQRCode()
        {
            // Create a barcode reader for decoding QR codes
            IBarcodeReader barCodeReader = new BarcodeReader {AutoRotate=false,TryInverted = true};
            barCodeReader.Options.TryHarder = true;
            
            
            // Start the webcam feed
            webcamTexture.Play();
            var snap = new Texture2D(webcamTexture.width, webcamTexture.height, TextureFormat.ARGB32, false);
            
            // Continue scanning until a QR code is found
            while (string.IsNullOrEmpty(QrCode))
            {
                try
                {
                    snap.SetPixels32(webcamTexture.GetPixels32()); // Capture a snapshot from the webcam
                    var Result = barCodeReader.Decode(snap.GetRawTextureData(), webcamTexture.width,
                        webcamTexture.height, RGBLuminanceSource.BitmapFormat.ARGB32);

                    if (Result != null)
                    {
                        QrCode = Result.Text; // Store the decoded QR code text

                        if (!string.IsNullOrEmpty(QrCode))
                        {
                            // Debug log the decoded QR code text
                            Debug.Log("DECODED TEXT FROM QR: " + QrCode);
                            
                            //! Check if the decoded text is a valid IP address
                            if (CheckIP(QrCode))
                            {
                                // Pass the IP to SceneReceiverModule
                                _sceneRec.receiveScene(QrCode, "5555");
                                
                                // Destroy the parent object (QR code scanner)
                                Destroy(transform.parent.gameObject);
                                break;
                            }
                            
                            // Reset the QR code text
                            QrCode = string.Empty;
                        }
                    }
                }
                catch (Exception ex)
                {
                    //! Log any exceptions
                    Debug.LogWarning(ex.Message);
                }
                // Yield to the next frame
                yield return null;
            }
        }

        //!
        //! Function to check if the input string is a valid IPv4 address
        //! @param input is the IP 
        //!
        private bool CheckIP(string input)
        {
            // IPv4 pattern for validation
            string ipv4Pattern = @"^(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.(25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)$";
            
            // Check if the input matches the IPv4 pattern
            return Regex.IsMatch(input, ipv4Pattern);
        }
    }
}

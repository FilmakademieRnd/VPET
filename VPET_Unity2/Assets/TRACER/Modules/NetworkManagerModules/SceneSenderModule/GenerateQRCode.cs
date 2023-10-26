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
//! @brief Generate a QR code
//! @author Alexandru Schwartz
//! @version 0
//! @date 26.10.2023


using UnityEngine;
using UnityEngine.UI;
using ZXing;
using ZXing.QrCode;
using TMPro;

public class GenerateQRCode : MonoBehaviour
{
    //!
    //! Public fields for input and output
    //!
    public string textInputField;
    public RawImage qrCodeImage;
    //public Texture2D logoTexture; // Reference to your logo texture

    //!
    //! Method to generate a QR code from the input text
    //!
    public void GenerateQRCodeFromText()
    {
        // Get the input text
        string text = textInputField;

        // Check if the input text is empty
        if (string.IsNullOrEmpty(text))
        {
            // Log a warning message
            Debug.LogWarning("Text input is empty. Please enter text to generate a QR code.");
            return;
        }

        // Generate the QR code texture
        Texture2D qrCodeTexture = GenerateQRCodeTexture(text, 256, 256);
        //qrCodeTexture = AddLogoToQRCode(qrCodeTexture, logoTexture);
        DisplayQRCode(qrCodeTexture);
    }

    //!
    //! Generate a QR code texture from the input text
    //!
    //! @param text is the server ip
    //! @param width is the width of the qr raw img
    //! @param height is the height of the qr raw img
    //!
    private Texture2D GenerateQRCodeTexture(string text, int width, int height)
    {
        // Set QR code encoding options
        QrCodeEncodingOptions qrCodeOptions = new QrCodeEncodingOptions
        {
            Width = width,
            Height = height
        };

        // Create a barcode writer for generating QR codes
        BarcodeWriter barcodeWriter = new BarcodeWriter();
        barcodeWriter.Format = BarcodeFormat.QR_CODE;
        barcodeWriter.Options = qrCodeOptions;

        // Write the QR code and get the pixel data
        Color32[] pixels = barcodeWriter.Write(text);

        // Create a texture from the pixel data
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, false);
        texture.SetPixels32(pixels);
        texture.Apply();
        return texture;
    }

    //!
    //! Display the generated QR code on the RawImage component
    //!
    private void DisplayQRCode(Texture2D qrCodeTexture)
    {
        qrCodeImage.texture = qrCodeTexture;
    }

    /*private Texture2D AddLogoToQRCode(Texture2D qrCodeTexture, Texture2D logoTexture)
    {
        if (qrCodeTexture == null || logoTexture == null)
        {
            Debug.LogError("QR code or logo texture is null.");
            return qrCodeTexture;
        }

        int logoSize = Mathf.Min(qrCodeTexture.width, qrCodeTexture.height) ;

        for (int x = 0; x < logoSize; x++)
        {
            for (int y = 0; y < logoSize; y++)
            {
                Color logoPixel = logoTexture.GetPixel(x, y);

                // Check if the logo pixel has enough alpha to be visible
                if (logoPixel.a > 0.5f)
                {
                    int qrX = (qrCodeTexture.width - logoSize) / 2 + x;
                    int qrY = (qrCodeTexture.height - logoSize) / 2 + y;

                    // Check if the QR code pixel is within bounds
                    if (qrX >= 0 && qrX < qrCodeTexture.width && qrY >= 0 && qrY < qrCodeTexture.height)
                    {
                        qrCodeTexture.SetPixel(qrX, qrY, logoPixel);
                    }
                }
            }
        }

        qrCodeTexture.Apply();
        return qrCodeTexture;
    }*/
    
}

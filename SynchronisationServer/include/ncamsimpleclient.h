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
#ifndef NCAMSIMPLECLIENT
#define NCAMSIMPLECLIENT

#endif // NCAMSIMPLECLIENT

#include <iostream>
#include <QTcpSocket>

#include "NcDataStreamBase.h"
#include "NcDataStreamClientBase.h"
#include "NcDataStreaming.h"
#include "NcDataStreamCamTrack.h"

/**
 * @brief Sample getting tracking information from Ncam-Reality software.
 *
 * This class inherits from NcDataStreamClientBase, and overload only usefull methods.
 * It displays the results of encoder into the terminal.
 * This class use the QT TCP socket to initialize the connection with the server.
 */
class NcamSimpleClient : public NcDataStreamClientBase
{
public:
    /**
     * @brief Default constructor.
     */
    NcamSimpleClient() :
        NcDataStreamClientBase()
    {
    }
    /**
     * @brief Default destructor.
     */
    virtual ~NcamSimpleClient()
    {
    }

    /**
     * @brief Map the buffer for packet access.
     * @param lSuccess Flag indicate if the mapping success.
     * @return The last packet read through the socket.
     * @warning No concurrent access allowed!
     */
    Packets_t* &MapPackets(bool &lSuccess)
    {
        lSuccess = true;
        return mDataBuffer;
    }
    /**
     * @brief Unmap the buffer.
     * @warning No concurrent access allowed!
     */
    void UnmapPackets()
    {
    }

    /**
     * @brief IsConnected is pure virtual, has to be overloaded as it depends on socket implementation.
     * @return True if the socket is opened.
     */
    bool IsConnected() const
    {
        return (mSocket != NULL);
    }

protected:
    /**
     * @brief InternalOpen This function has to be overloaded with the specific socket openning process.
     * @return True if the connection can be opened, false if the socket is already openned or if the socket cannot be openned.
     */
    bool InternalOpen()
    {
        if( IsConnected() )
            return false;
        mSocket = new QTcpSocket();
        // Disable Nagle's algorithm
        mSocket->setSocketOption( QAbstractSocket::LowDelayOption, true );
        // Connect to the server
        mSocket->connectToHost( QString( GetIpAddress() ), GetPort() );

        if( mSocket->waitForConnected( 1000 ) )
        {
            return true;
        }
        // Show the QT error message
        qDebug() << mSocket->error();
        delete mSocket;
        mSocket = NULL;
        return false;
    }
    /**
     * @brief InternalClose This function has to be overloaded with socket-depend close process.
     * @return True if the socket has been closed, false if the socket wasn't opened.
     */
    bool InternalClose()
    {
        if( !IsConnected() )
            return false;
        mSocket->close();
        delete mSocket;
        mSocket = NULL;
        return true;
    }
    /**
     * @brief InternalRead Read a buffer of bytes.
     * @param data The buffer used to store the read bytes.
     * @param maxlen The maximum size allowed for the data buffer.
     * @return The number of bytes read.
     */
    ssize_t InternalRead(uint8_t *data, const size_t &maxlen)
    {
        ssize_t lSizeRead = -1;
        if (IsConnected())
        {
            bool lSuccess = (mSocket->bytesAvailable()>0);
            if (false == lSuccess)
                lSuccess = mSocket->waitForReadyRead(1000);
            if (lSuccess)
            {
                lSizeRead = mSocket->read((char*)data, maxlen);
            }
        }
        return lSizeRead;
    }
    /**
     * @brief InternalWrite Write to the socket (for communication to the server).
     * @param data The buffer to write through the socket.
     * @param maxlen The size of the buffer.
     * @return The number of bytes written, or -1 if the writting failed.
     */
    ssize_t InternalWrite(const uint8_t *data, const size_t &maxlen)
    {
        ssize_t lSizeWritten = -1;
        if (IsConnected())
        {
            bool lSuccess = false;
            lSizeWritten = mSocket->write((char*)data, maxlen);
            if (lSizeWritten>0)
            {
                lSuccess = mSocket->waitForBytesWritten(1000);
            }
            if (false == lSuccess)
            {
                lSizeWritten = -1;
            }
        }
        return lSizeWritten;
    }

protected:
    Packets_t* mDataBuffer; ///< Buffer of packets.
    QTcpSocket* mSocket;   ///< Socket object.

protected:
    /**
     * @brief OnStartStreamingError overloaded error callback when opening the connection.
     * @param lErrorDecription The error message returned by the API.
     */
    void OnStartStreamingError(const std::string &lErrorDecription)
    {
        std::cerr << std::endl << __FUNCTION__ << ": " << lErrorDecription << std::endl;
    }
    /**
     * @brief OnStopStreamingError overloaded error callback when stoping the connection.
     * @param lErrorDecription The error message returned by the API.
     */
    void OnStopStreamingError(const std::string &lErrorDecription)
    {
        std::cerr << std::endl << __FUNCTION__ << ": " << lErrorDecription << std::endl;
    }
    /**
     * @brief OnDoStreamingError overloading error callback when sending/receiving packets through the socket.
     * @param lErrorDecription The error message returned by the API.
     */
    void OnDoStreamingError(const std::string &lErrorDecription)
    {
        std::cerr << std::endl << __FUNCTION__ << ": " << lErrorDecription << std::endl;
    }
};

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
#include "ncamadapter.h"
//#include "zmq_utils.h"
#include <QThread>

NcamAdapter::NcamAdapter(QString ip, QString port, QString serverIP, zmq::context_t* context)
{
    timeCode = 0;
    ip_address = ip;
    ip_port = port.toInt();
    context_ = context;
    serverIP_ = serverIP;
}

void NcamAdapter::handlePacket( NcamSimpleClient* simpleClient )
{
    bool lIsASuccess = false;
    const Packets_t* lPackets = simpleClient->MapPackets(lIsASuccess);
    if (lIsASuccess)
    {
        {
            NcDataStreamBase::PacketType_t lSearchedPacket = NcDataStreamBase::CameraTracking;
            NcDataStreamBase::PacketType_t lSearchedPacket2 = NcDataStreamBase::OpticalParameters;
            auto lIter = lPackets->find(lSearchedPacket);
            auto lIter2 = lPackets->find(lSearchedPacket2);
            std::string message, message1, message2;
            if (lIter !=lPackets->end())
            {
                const NcDataStreamCamTrack* lPacket = static_cast<const NcDataStreamCamTrack*> (lIter->second);
                const NcDataStreamCamTrack::RigidTransfoPacket& packet = lPacket->GetData().mCamFromWorld;
                timeCode = lPacket->GetData().mTimeCode;
                //float* inv = new float[9];
                const double* inv = packet.mRotation;
                //invert3x3(inv, packet.mRotation);
                message1 = QString("ncam ncam|t|cam|"+
                                   QString::number(float(       inv[0]*packet.mTranslation[0] + inv[1]*packet.mTranslation[1] + inv[2]*packet.mTranslation[2]))+"|"+
                                   QString::number(float(-1.0f*(inv[3]*packet.mTranslation[0] + inv[4]*packet.mTranslation[1] + inv[5]*packet.mTranslation[2])))+"|"+
                                   QString::number(float(-1.0f*(inv[6]*packet.mTranslation[0] + inv[7]*packet.mTranslation[1] + inv[8]*packet.mTranslation[2])))).toLatin1().data();
                zmq::message_t msgT((void*)message1.c_str(),message1.size(),NULL);
                socket_->send(msgT);

                message2 = QString("ncam ncam|r|cam|"+QuaternionFromRotationMatrix(packet.mRotation)).toLatin1().data();
                zmq::message_t msgR((void*)message2.c_str(),message2.size(),NULL);
                socket_->send(msgR);
            }
            if(lIter2 !=lPackets->end())
            {
                const NcDataStreamOpticalParameters* lPacket = static_cast<const NcDataStreamOpticalParameters*> (lIter2->second);
                message = QString("ncam ncam|f|cam|"+QString::number(float(*lPacket->GetData().mFovInDegrees))).toLatin1().data();
                zmq::message_t msg((void*)message.c_str(),message.size(),NULL);
                socket_->send(msg);
            }

        }
    }
    simpleClient->UnmapPackets();
}

void NcamAdapter::signal_callback_handler(int /*signum*/)
{
    // Properly close the streaming
    if( nullptr != simpleClient )
    {
        simpleClient->StopStreaming();
        delete simpleClient;
        simpleClient = nullptr;
    }

    std::cout << std::endl;

   // Terminate program
   exit(0);
}

void NcamAdapter::run()
{
    std::cout << "ncam server running" << thread()->currentThreadId() << std::endl;

    // ip_port = 38860;

    simpleClient = new NcamSimpleClient();

    simpleClient->SetConnectionParameters( ip_address.toLatin1().data(), ip_port);

    while(true)
    {
        NcDataStreamBase::PacketType_t packetType = 0;

        packetType |= NcDataStreamBase::CameraTracking;
        //packetType |= NcDataStreamBase::DepthImage;
        //packetType |= NcDataStreamBase::FilmImage;
        //packetType |= NcDataStreamBase::CompositeImage;
        //packetType |=NcDataStreamBase::DistortMap;
        packetType |=NcDataStreamBase::OpticalParameters;

        simpleClient->AskForPackets(packetType);
        if(simpleClient->StartStreaming())
        {

            socket_ = new zmq::socket_t(*context_,ZMQ_PUB);
            socket_->connect(QString("tcp://"+serverIP_+":5557").toLatin1().data());

            while(simpleClient->DoStreaming())
            {
                handlePacket( simpleClient );
            }

            simpleClient->StopStreaming();
        }
        //QThread::sleep(5);
    }
    delete simpleClient;
    simpleClient = nullptr;
}


QString NcamAdapter::QuaternionFromRotationMatrix (const double* rotationMatrix)
{
    /* Algorithm from Ken Shoemake article in 1987 SIGGRAPH course
    notes article "Quaternion Calculus and Fast Animation". */

    //std::cout << rotationMatrix[0] << " " << rotationMatrix[1] << " " << rotationMatrix[2] << std::endl;
    //std::cout << rotationMatrix[3] << " " << rotationMatrix[4] << " " << rotationMatrix[5] << std::endl;
    //std::cout << rotationMatrix[6] << " " << rotationMatrix[7] << " " << rotationMatrix[8] << std::endl;


    float fTrace = rotationMatrix[0]+rotationMatrix[4]+rotationMatrix[8];
    float fRoot;
    float quaternion[4]; //[x, y, z, w]

    if ( fTrace > 0.0 )
    {
        fRoot = qSqrt(fTrace + 1.0f);
        quaternion[3] = 0.5f * fRoot;
        fRoot = 0.5f / fRoot;
        quaternion[0] = (rotationMatrix[7] - rotationMatrix[5]) * fRoot;
        quaternion[1] = (rotationMatrix[2] - rotationMatrix[6]) * fRoot;
        quaternion[2] = (rotationMatrix[3] - rotationMatrix[1]) * fRoot;
    }
    else
    {
        int next[3];
        next[0] = 1;
        next[1] = 2;
        next[2] = 0;
        int i = 0;
        if ( rotationMatrix[4] > rotationMatrix[0] )
            i = 1;
        if ( rotationMatrix[8] > rotationMatrix[(i*3)+i] )
            i = 2;
        int j = next[i];
        int k = next[j];

        fRoot = qSqrt(rotationMatrix[(i*3)+i]-rotationMatrix[(j*3)+j]-rotationMatrix[(k*3)+k] + 1.0f);

        float r[3];
        r[0] = 0;
        r[1] = 0;
        r[2] = 0;

        r[i] = 0.5f * fRoot;
        fRoot = 0.5f / fRoot;
        quaternion[3] = (rotationMatrix[(k*3)+j] - rotationMatrix[(j*3)+k]) * fRoot;
        r[j] = (rotationMatrix[(j*3)+i] + rotationMatrix[(i*3)+j]) * fRoot;
        r[k] = (rotationMatrix[(k*3)+i] + rotationMatrix[(i*3)+k]) * fRoot;

        quaternion[0] = r[0];
        quaternion[1] = r[1];
        quaternion[2] = r[2];

        //std::cout << quaternion[0] << " " << quaternion[1] << " " << quaternion[2] << " " << quaternion[3] << std::endl;
    }

    return QString(QString::number(quaternion[0])+"|"+QString::number(quaternion[1])+"|"+QString::number(quaternion[2])+"|"+QString::number(quaternion[3]));
}

void NcamAdapter::invert3x3(float* inv, const double* m)
{
    inv[0] = m[4]*m[8] -
        m[5]*m[7];
    inv[1] = m[2]*m[7] -
        m[1]*m[8];
    inv[2] = m[1]*m[5] -
        m[2]*m[4];
    inv[3] = m[5]*m[6] -
        m[3]*m[8];
    inv[4] = m[0]*m[8] -
        m[2]*m[6];
    inv[5] = m[2]*m[3] -
        m[0]*m[5];
    inv[6] = m[3]*m[7] -
        m[4]*m[6];
    inv[7] = m[1]*m[6] -
        m[0]*m[7];
    inv[8] = m[0]*m[4] -
        m[1]*m[3];

    qreal fInvDet = 1.0f/(
        m[0]*inv[0] +
        m[1]*inv[3]+
        m[2]*inv[6]);

    for (size_t iRow = 0; iRow < 3; iRow++)
    {
        for (size_t iCol = 0; iCol < 3; iCol++)
            inv[(iRow*3)+iCol] *= fInvDet;
    }
}

/*float* NcamAdapter::mulQuatPos(float* quat, float* vec)
{
    float num = quat[0] * 2.0f;
    float num2 = quat[1] * 2.0f;
    float num3 = quat[2] * 2.0f;
    float num4 = quat[0] * num;
    float num5 = quat[1] * num2;
    float num6 = quat[2] * num3;
    float num7 = quat[0] * num2;
    float num8 = quat[0] * num3;
    float num9 = quat[1] * num3;
    float num10 = quat[3] * num;
    float num11 = quat[3] * num2;
    float num12 = quat[3] * num3;
    float* result = new float[3];
    result[0] = (1.0f - (num5 + num6)) * vec[0] + (num7 - num12) * vec[1] + (num8 + num11) * vec[2];
    result[1] = (num7 + num12) * vec[0] + (1.0f - (num4 + num6)) * vec[1] + (num9 - num10) * vec[2];
    result[2] = (num8 - num11) * vec[0] + (num9 + num10) * vec[1] + (1.0f - (num4 + num5)) * vec[2];
    return result;
}*/

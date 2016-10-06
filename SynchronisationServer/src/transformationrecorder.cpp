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
the terms of the GNU Lesser General Public License as published by the Free Software
Foundation; version 2.1 of the License.

This program is distributed in the hope that it will be useful, but WITHOUT
ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS
FOR A PARTICULAR PURPOSE. See the GNU Lesser General Public License for more details.

You should have received a copy of the GNU Lesser General Public License along with
this program; if not, write to the Free Software Foundation, Inc., 59 Temple
Place - Suite 330, Boston, MA 02111-1307, USA, or go to
http://www.gnu.org/licenses/old-licenses/lgpl-2.1.html
-----------------------------------------------------------------------------
*/
#include "transformationrecorder.h"
#include <iostream>
#include <QThread>

TransformationRecorder::TransformationRecorder(QString serverIP, zmq::context_t* context, QList<QStringList> *i_messagesStorage, QMutex *i_mutex, NcamAdapter *i_ncamAdapter ) //, RecordWriter* i_recordWriter )
{
    serverIP_ = serverIP;
    context_ = context;
    timer.start();
    lastTime = 0;
    messagesStorage = i_messagesStorage;
    mutexmain = i_mutex;
    ncamAdapter = i_ncamAdapter;
    // erecordWriter = i_recordWriter;
    isRecording = false;
    lastTimeCode = 0;
}

void TransformationRecorder::run()
{
    const std::string topicstr = "client";
    const char* topic = topicstr.c_str();
    socket_ = new zmq::socket_t(*context_,ZMQ_SUB);
    socket_->connect(QString("tcp://"+serverIP_+":5556").toLatin1().data());
    socket_->setsockopt(ZMQ_SUBSCRIBE,topic,sizeof(char)*topicstr.length());

    // sendSocket_ = new zmq::socket_t(*context_,ZMQ_PUB);
    // sendSocket_->connect(QString("tcp://"+serverIP_+":5557").toLatin1().data());

    std::cout << "recorder running in thread " << thread()->currentThreadId() << std::endl;

    while(true)
    {
        zmq::message_t message;

        socket_->recv(&message);

        msg_ = QString::fromStdString(std::string(static_cast<char*>(message.data()), message.size()));

        // std::cout << msg_.toLatin1().data() << std::endl;

        QStringList splitMsg = msg_.split("|");
        if(splitMsg[splitMsg.length()-1] == "physics") splitMsg.removeAt(splitMsg.length()-1);

        if ( splitMsg[0] == "recordstart" )
        {
            std::cout << "Start recording!" << std::endl;
            isRecording = true;
            // clean buffer
            mutexmain->lock();
            messagesStorage->clear();
            mutexmain->unlock();
        }
        else if ( splitMsg[0] == "recordstop" )
        {
            std::cout << "Stop recording!" << std::endl;
            isRecording = false;
            // send force message to writer to write rest of the data
            // recordWriter->forceWrite = true;
        }

        if ( ncamAdapter->timeCode != lastTimeCode )
        {
            // add time code
            splitMsg.insert(0, QString::number( ncamAdapter->timeCode ) );

            if ( isRecording )
            {
                // write everything
                mutexmain->lock();
                // std::cout << splitMsg.join("|").toStdString()  << std::endl;
                messagesStorage->append(splitMsg);
                mutexmain->unlock();
            }

            // std::cout << "messagesStorage->length() after appending " << messagesStorage->size() << std::endl;

            /*
             * Check if object already in list and overwrite existing transform (previous version)
            bool found = false;
            if(splitMsg.length() > 3)
            {
                for(int i = 0; i < messagesStorage.length(); i++)
                {
                    if(messagesStorage[i][2] == splitMsg[2] || splitMsg[2] != "cam")
                    {
                        if(messagesStorage[i][1] == splitMsg[1])
                        {
                            messagesStorage[i] = splitMsg;
                            found = true;
                            break;
                        }
                    }
                }
                if (!found)
                {
                    messagesStorage.append(splitMsg);
                }
            }
            */

            /*if (timer.elapsed() - lastTime > 2000)
            {
                std::cout << "sending initial updates: " << messagesStorage.length() << std::endl;
                for(int i = 0; i < messagesStorage.length(); i++)
                {
                    std::string msg = messagesStorage[i].join("|").toLatin1().data();
                    std::cout << msg << std::endl;
                    zmq::message_t message((void*)msg.c_str(),msg.size(),NULL);
                    sendSocket_->send(message);
                }
                lastTime = timer.elapsed();
            }*/


            lastTimeCode = ncamAdapter->timeCode;

        }


    }
}

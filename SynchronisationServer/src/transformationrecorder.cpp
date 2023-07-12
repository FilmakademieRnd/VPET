/*
-----------------------------------------------------------------------------
This source file is part of VPET - Virtual Production Editing Tool
http://vpet.research.animationsinstitut.de/
http://github.com/FilmakademieRnd/VPET

Copyright (c) 2023 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

This project has been initiated in the scope of the EU funded project
Dreamspace under grant agreement no 610005 in the years 2014, 2015 and 2016.
http://dreamspaceproject.eu/
Post Dreamspace the project has been further developed on behalf of the
research and development activities of Animationsinstitut.

The VPET components Scene Distribution and Synchronization Server are intended
for research and development purposes only. Commercial use of any kind is not
permitted.

There is no support by Filmakademie. Since the Scene Distribution and
Synchronization Server are available for free, Filmakademie shall only be
liable for intent and gross negligence; warranty is limited to malice. Scene
Distribution and Synchronization Server may under no circumstances be used for
racist, sexual or any illegal purposes. In all non-commercial productions,
scientific publications, prototypical non-commercial software tools, etc.
using the Scene Distribution and/or Synchronization Server Filmakademie has
to be named as follows: “VPET-Virtual Production Editing Tool by Filmakademie
Baden-Württemberg, Animationsinstitut (http://research.animationsinstitut.de)“.

In case a company or individual would like to use the Scene Distribution and/or
Synchronization Server in a commercial surrounding or for commercial purposes,
software based on these components or any part thereof, the company/individual
will have to contact Filmakademie (research<at>filmakademie.de).
-----------------------------------------------------------------------------
*/
#include "transformationrecorder.h"
#include <iostream>
#include <QThread>

TransformationRecorder::TransformationRecorder(QString serverIP, zmq::context_t* context, QList<QStringList> *i_messagesStorage, QMutex *i_mutex) //, RecordWriter* i_recordWriter )
{
    serverIP_ = serverIP;
    context_ = context;
    timer.start();
    lastTime = 0;
    messagesStorage = i_messagesStorage;
    mutexmain = i_mutex;
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
    }
}

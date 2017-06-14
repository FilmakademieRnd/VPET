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
#include "mainapp.h"

MainApp::MainApp(QString ownIP, QString ncamIP, QString ncamPort)
{
    context_ = new  zmq::context_t(1);
    ownIP_ = ownIP;
    ncamIP_ = ncamIP;
    ncamPort_ = ncamPort;
    isRecording = false;
}

void MainApp::run()
{
    //create Thread to receive zeroMQ messages from tablets
    QThread* zeroMQHandlerThread = new QThread();
    ZeroMQHandler* zeroMQHandler = new ZeroMQHandler(ownIP_, context_);
    zeroMQHandler->moveToThread(zeroMQHandlerThread);
    QObject::connect( zeroMQHandlerThread, SIGNAL(started()), zeroMQHandler, SLOT(run()));
    zeroMQHandlerThread->start();
    zeroMQHandler->requestStart();


    NcamAdapter* ncamAdapter = new NcamAdapter(ncamIP_, ncamPort_, ownIP_, context_);
    if(ncamIP_ != "" && ncamPort_ != "")
    {
        QThread* ncamThread = new QThread();
        ncamAdapter->moveToThread(ncamThread);
        QObject::connect( ncamThread, SIGNAL(started()), ncamAdapter, SLOT(run()));
        ncamThread->start();
    }


    /*
    RecordWriter* recordWriter = new RecordWriter( &messagesStorage, &m_mutex );
    QThread* writeThread = new QThread();
    recordWriter->moveToThread( writeThread );
    QObject::connect( writeThread, SIGNAL( started() ), recordWriter, SLOT( run() ) );
    writeThread->start();

    QThread* recorderThread = new QThread();
    TransformationRecorder* transformationRecorder = new TransformationRecorder(ownIP_, context_, &messagesStorage, &m_mutex, ncamAdapter );//, recordWriter);
    transformationRecorder->moveToThread(recorderThread);
    QObject::connect( recorderThread, SIGNAL(started()), transformationRecorder, SLOT(run()));
    recorderThread->start();
    */

}

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
#include "mainapp.h"

MainApp::MainApp(QString ownIP, QString ncamIP, QString ncamPort, bool debug)
{
    context_ = new  zmq::context_t(1);
    ownIP_ = ownIP;
    ncamIP_ = ncamIP;
    ncamPort_ = ncamPort;
    debug_ = debug;
    isRecording = false;
}

void MainApp::run()
{
    //create Thread to receive zeroMQ messages from tablets
    QThread* zeroMQHandlerThread = new QThread();
    ZeroMQHandler* zeroMQHandler = new ZeroMQHandler(ownIP_, debug_ , context_);
    zeroMQHandler->moveToThread(zeroMQHandlerThread);
    QObject::connect( zeroMQHandlerThread, SIGNAL(started()), zeroMQHandler, SLOT(run()));
    zeroMQHandlerThread->start();
    zeroMQHandler->requestStart();

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

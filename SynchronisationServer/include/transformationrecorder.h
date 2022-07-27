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
#ifndef TRANSFORMATIONRECORDER_H
#define TRANSFORMATIONRECORDER_H

#include <QObject>
#include <QMutex>
#include <nzmqt/nzmqt.hpp>
#include <QElapsedTimer>
#include <QStringList>
#include "recordWriter.h"

class TransformationRecorder : public QObject
{
    Q_OBJECT
public:
    explicit TransformationRecorder(QString serverIP,
                                    zmq::context_t* context,
                                    QList<QStringList>* i_messagesStorage,
                                    QMutex* i_mutex);

private:
    QString serverIP_;
    zmq::context_t* context_;
    zmq::socket_t* socket_;
    zmq::socket_t* sendSocket_;
    QList<QStringList>* messagesStorage;

    QElapsedTimer timer;
    int lastTime;

    QString msg_;

    QMutex* mutexmain;

    // RecordWriter* recordWriter;

    bool isRecording;

    int lastTimeCode;

signals:

public slots:
    void run();
};

#endif // TRANSFORMATIONRECORDER_H

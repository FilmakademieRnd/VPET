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
#ifndef TRANSFORMATIONRECORDER_H
#define TRANSFORMATIONRECORDER_H

#include <QObject>
#include <QMutex>
#include <nzmqt/nzmqt.hpp>
#include <QTime>
#include <QStringList>

#include "ncamadapter.h"
#include "recordWriter.h"

class TransformationRecorder : public QObject
{
    Q_OBJECT
public:
    explicit TransformationRecorder(QString serverIP,
                                    zmq::context_t* context,
                                    QList<QStringList>* i_messagesStorage,
                                    QMutex* i_mutex,
                                    NcamAdapter* i_ncamAdapter );

private:
    QString serverIP_;
    zmq::context_t* context_;
    zmq::socket_t* socket_;
    zmq::socket_t* sendSocket_;
    QList<QStringList>* messagesStorage;

    QTime timer;
    int lastTime;

    QString msg_;

    QMutex* mutexmain;

    NcamAdapter* ncamAdapter;

    // RecordWriter* recordWriter;

    bool isRecording;

    int lastTimeCode;

signals:

public slots:
    void run();
};

#endif // TRANSFORMATIONRECORDER_H

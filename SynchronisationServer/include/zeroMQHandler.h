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
#ifndef ZEROMQHANDLER_H
#define ZEROMQHANDLER_H

#include <QObject>
#include <QMutex>
#include <nzmqt/nzmqt.hpp>

class ZeroMQHandler : public QObject
{
    Q_OBJECT
public:
    explicit ZeroMQHandler(QString IPAdress = "", zmq::context_t* context = NULL);

    //request this process to start working
    void requestStart();

    //request this process to stop working
    void requestStop();

private:
    //if true process is stopped
    bool _stop;

    //if true process is running
    bool _working;

    //protect access to _stop
    QMutex mutex;

    //zeroMQ socket
    zmq::socket_t* socket_;
    zmq::socket_t* sender_;

    //zeroMQ context
    zmq::context_t* context_;

    //server IP
    QString IPadress;

signals:
    //signal emitted when process requests to work
    void startRequested();

    //signal emitted when process is finished
    void stopped();

public slots:
    //execute operations
    void run();
};

#endif // ZEROMQHANDLER_H

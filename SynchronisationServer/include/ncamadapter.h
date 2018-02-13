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
#ifndef NCAMADAPTER_H
#define NCAMADAPTER_H

#include <QObject>
#include <ncamsimpleclient.h>
#include <QThread>
#include <nzmqt/nzmqt.hpp>
#include <qmath.h>

class NcamAdapter : public QObject
{
    Q_OBJECT
public:
    explicit NcamAdapter(QString ip, QString port, QString serverIP, zmq::context_t* context);
    uint32_t timeCode;

private:
    uint16_t ip_port;
    QString ip_address;
    QString serverIP_;
    QString port_;
    NcamSimpleClient* simpleClient;

    zmq::context_t* context_;
    zmq::socket_t* socket_;

    void handlePacket( NcamSimpleClient* simpleClient );
    void signal_callback_handler(int /*signum*/);
    QString QuaternionFromRotationMatrix (const double* rotationMatrix);
    void invert3x3(float* inv, const double* m);

signals:

public slots:
    void run();
};

#endif // NCAMADAPTER_H

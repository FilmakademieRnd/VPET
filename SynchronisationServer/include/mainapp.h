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
#ifndef MAINAPP_H
#define MAINAPP_H

#include <QObject>
#include <QThread>
#include <QMutex>
#include <zeroMQHandler.h>
#include <ncamadapter.h>
#include <transformationrecorder.h>
#include <iostream>

#include "recordWriter.h"

class MainApp : public QObject
{
    Q_OBJECT
public:
    explicit MainApp(QString ownIP, QString ncamIP, QString ncamPort);

    void run();

public:
    QList<QStringList> messagesStorage;

private:
    QString ownIP_;
    QString ncamIP_;
    QString ncamPort_;
    zmq::context_t* context_;
    QMutex m_mutex;
    bool isRecording;

signals:

public slots:

};

#endif // MAINAPP_H

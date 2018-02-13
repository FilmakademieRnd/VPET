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
#ifndef OBJECTSTATEHANDLER_H
#define OBJECTSTATEHANDLER_H

#include <QObject>
#include <QMutex>
#include <nzmqt/nzmqt.hpp>

class ObjectStateHandler : public QObject
{
    Q_OBJECT
public:
    explicit ObjectStateHandler(zmq::message_t *message);

private:
    //protect access to _stop
    QMutex mutex_;

	//the pointer to last message
	zmq::message_t *message_;

	//map of last states
	QMap<QString, QString> objectStateMap_;

public slots:
    //execute operations
    void run();
};

#endif // ZEROMQHANDLER_H

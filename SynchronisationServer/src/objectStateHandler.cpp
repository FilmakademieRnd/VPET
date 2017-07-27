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
#include "objectStateHandler.h"

#include <QThread>
#include <QDebug>
#include <iostream>

ObjectStateHandler::ObjectStateHandler(zmq::message_t *message)
{
    
}


void ObjectStateHandler::run()
{
   qDebug() << "Starting ObjectStateHandler in Thread " << thread()->currentThreadId();

    while(true) {
        mutex_.lock();
		QString messageCopy = QString::fromStdString(std::string(static_cast<char*>(message_->data()), message_->size()));
		mutex_.unlock();
		
		QString key = messageCopy.section('|', 2, 3);

		mutex_.lock();
		objectStateMap_.insert(key, messageCopy);
		mutex_.unlock();
    }

    qDebug() << "ObjectStateHandler process stopped in Thread " << thread()->currentThreadId();
}



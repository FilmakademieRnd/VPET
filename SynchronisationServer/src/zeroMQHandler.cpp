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
#include "zeroMQHandler.h"

#include <QThread>
#include <QDebug>
#include <iostream>

ZeroMQHandler::ZeroMQHandler(QString ip , zmq::context_t* context, zmq::message_t *message)
{
    IPadress = ip;
    context_ = context;
	message = &message_;
    _stop = false;
    _working =false;
}

void ZeroMQHandler::requestStart()
{
    mutex.lock();
    _working = true;
    _stop = false;
    qDebug()<<"ZeroMQHandler requested to start in Thread "<<thread()->currentThreadId();
    mutex.unlock();

    emit startRequested();
}

void ZeroMQHandler::requestStop()
{
    mutex.lock();
    if (_working) {
        _stop = true;
        qDebug()<<"ZeroMQHandler stopping in Thread "<<thread()->currentThreadId();
    }
    mutex.unlock();
}

void ZeroMQHandler::run()
{
    socket_ = new zmq::socket_t(*context_,ZMQ_SUB);
    socket_->bind(QString("tcp://"+IPadress+":5557").toLatin1().data());
    socket_->setsockopt(ZMQ_SUBSCRIBE,"client",0);
    socket_->setsockopt(ZMQ_SUBSCRIBE,"ncam",0);
    socket_->setsockopt(ZMQ_SUBSCRIBE,"recorder",0);

    sender_ = new zmq::socket_t(*context_,ZMQ_PUB);
    sender_->bind(QString("tcp://"+IPadress+":5556").toLatin1().data());

    qDebug()<<"Starting ZeroMQHandler in Thread " << thread()->currentThreadId();

    while(true) {

        // checks if process should be aborted
        mutex.lock();
		bool stop = _stop;
		mutex.unlock();

		socket_->recv(&message_);

		QString stringMessage = QString::fromStdString(std::string(static_cast<char*>(message_.data()), message_.size()));
		QString key = stringMessage.section('|', 1, 2);
		if (key == "udOb") {
			foreach(const QString &objectState, objectStateMap) {
				const QByteArray osByteArray = objectState.toLocal8Bit();
				sender_->send(osByteArray.constData(), osByteArray.length());
			}
        }
		else {
            if(key.at(0) != 'l')
				objectStateMap.insert(key, "client 001|" + stringMessage.section('|', 1, -1));
            sender_->send(message_);
		}

#ifdef _DEBUG
		// 4 Debug
		std::cout << "Map Size: " << objectStateMap.size() << std::endl;
		std::cout << key.toStdString() << " : " << std::endl;
		std::cout << stringMessage.toStdString() << std::endl;
#endif

        if (stop) {
            qDebug()<<"Stopping ZeroMQHandler in Thread "<<thread()->currentThreadId();
            break;
        }
    }

    // Set _working to false -> process cannot be aborted anymore
    mutex.lock();
    _working = false;
    mutex.unlock();

    qDebug()<<"ZeroMQHandler process stopped in Thread "<<thread()->currentThreadId();

    emit stopped();
}

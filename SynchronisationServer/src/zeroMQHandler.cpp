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
#include "zeroMQHandler.h"

#include <QThread>
#include <QDebug>
#include <iostream>

ZeroMQHandler::ZeroMQHandler(QString ip , zmq::context_t* context)
{
    IPadress = ip;
    context_ = context;
    _stop = false;
    _working =false;
}

void ZeroMQHandler::requestStart()
{
    mutex.lock();
    _working = true;
    _stop = false;
    qDebug()<<"ZeroMQHandler requested to start";// in Thread "<<thread()->currentThreadId();
    mutex.unlock();

    emit startRequested();
}

void ZeroMQHandler::requestStop()
{
    mutex.lock();
    if (_working) {
        _stop = true;
        qDebug()<<"ZeroMQHandler stopping";// in Thread "<<thread()->currentThreadId();
    }
    mutex.unlock();
}

int ZeroMQHandler::CharToInt(const char* buf)
{
  int val;
  std::memcpy(&val, buf, 4);
  return val;
}

void ZeroMQHandler::run()
{
    socket_ = new zmq::socket_t(*context_,ZMQ_SUB);
    int timeout = 5000;
    socket_->setsockopt(ZMQ_RCVTIMEO,&timeout,sizeof (int));
    socket_->bind(QString("tcp://"+IPadress+":5557").toLatin1().data());
    socket_->setsockopt(ZMQ_SUBSCRIBE,"client",0);
    socket_->setsockopt(ZMQ_SUBSCRIBE,"ncam",0);
    socket_->setsockopt(ZMQ_SUBSCRIBE,"recorder",0);

    sender_ = new zmq::socket_t(*context_,ZMQ_PUB);
    sender_->bind(QString("tcp://"+IPadress+":5556").toLatin1().data());

    qDebug()<<"Starting ZeroMQHandler";// in Thread " << thread()->currentThreadId();

    while(true) {

        // checks if process should be aborted
        mutex.lock();
        bool stop = _stop;
        mutex.unlock();

        zmq::message_t message;

        socket_->recv(&message);

        //check if recv timed out
        if(message.size() != 0)
        {
            char* rawData = static_cast<char*>(message.data());
            QByteArray msgArray = QByteArray::fromRawData(rawData, static_cast<int>(message.size()));

            char clientID = rawData[0];
            ParameterType paramType = static_cast<ParameterType>(rawData[1]);
            int objectID = CharToInt(&rawData[2]);
            QByteArray msgKey = QByteArray::fromRawData(rawData++, 5); //combination of ParamType & objectID

            //QString debugMsg = "";
            //foreach (const char c, msgArray){
            //    debugMsg += QString::number((int)c) + " ";
            //}
            //std::cout << debugMsg.toStdString() << std::endl;

            //update ping timeout
            if(pingMap.contains(clientID))
            {
                pingMap[clientID]->restart();
            }
            else
            {
                QTime* t = new QTime();
                t->start();
                pingMap.insert(clientID,t);
                std::cout << "New client registered: " << 256 + (int) clientID << std::endl;
            }

            if (paramType == RESENDUPDATE) {
                foreach(const QString &objectState, objectStateMap) {
                    const QByteArray osByteArray = objectState.toLocal8Bit();
                    sender_->send(osByteArray.constData(), static_cast<size_t>(osByteArray.length()));
                }
            }
            else if (paramType == LOCK){
                //store locked object for each client
                lockMap.insert(clientID,objectID);
                objectStateMap.insert(msgKey, msgArray.replace(0,1,&targetHostID,1));
                sender_->send(message);
            }
            else if (paramType != PING){
                objectStateMap.insert(msgKey, msgArray.replace(0,1,&targetHostID,1));
                sender_->send(message);
            }
        }

        //check if ping timed out for any client
        foreach(QTime* time, pingMap) {
            if(time->elapsed() > 3000)
            {
                //connection to client lost
                char clientID = pingMap.key(time);
                std::cout << "Lost connection to: " << 256 + (int) clientID << std::endl;
                pingMap.remove(clientID);
                //check if client had lock
                if(lockMap.contains(clientID))
                {
                    //release lock
                    char* lockReleaseMsg = static_cast<char*>(malloc(7));
                    *lockReleaseMsg = 0;
                    lockReleaseMsg++;
                    *lockReleaseMsg = static_cast<char>(LOCK);
                    lockReleaseMsg++;
                    void* intPtr = (char*) &lockMap[clientID];
                    memcpy(lockReleaseMsg,intPtr,sizeof(int));
                    lockReleaseMsg+=4;
                    *lockReleaseMsg = static_cast<char>(false);
                    std::cout << "Resetting lock!";
                    lockReleaseMsg-=6;
                    QByteArray msgKey = QByteArray::fromRawData(lockReleaseMsg+1, 5);
                    QByteArray msgArray = QByteArray::fromRawData(lockReleaseMsg, 7);

                    objectStateMap.insert(msgKey, msgArray);
                    sender_->send(msgArray.constData(), static_cast<size_t>(msgArray.length()));

                    lockMap.remove(clientID);

                    if (lockReleaseMsg) {
                        delete lockReleaseMsg;
                        lockReleaseMsg = 0;
                    }
                }
            }
        }

        if (stop) {
            qDebug()<<"Stopping ZeroMQHandler";// in Thread "<<thread()->currentThreadId();
            break;
        }
    }

    // Set _working to false -> process cannot be aborted anymore
    mutex.lock();
    _working = false;
    mutex.unlock();

    qDebug()<<"ZeroMQHandler process stopped";// in Thread "<<thread()->currentThreadId();

    emit stopped();
}

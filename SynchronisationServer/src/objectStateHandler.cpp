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



/*
-----------------------------------------------------------------------------
This source file is part of VPET - Virtual Production Editing Tool
http://vpet.research.animationsinstitut.de/
http://github.com/FilmakademieRnd/VPET

Copyright (c) 2023 Filmakademie Baden-Wuerttemberg, Animationsinstitut R&D Lab

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
#include <iostream>

#include "recordWriter.h"
#include <algorithm>
#include <QDir>
#include <QTextStream>
#include <QThread>

RecordWriter::RecordWriter(QList<QStringList> *i_messagesStorage, QMutex* i_mutex):
    RECORDSIZE(1024),
    filecount(0),
    forceWrite(false)
{
    messagesStorage = i_messagesStorage;

    filesrc = QDir::tempPath() + "/" + "sync_%1.txt";
    mutexmain = i_mutex;

}

void RecordWriter::run()
{
    std::cout << "writer running in thread " << thread()->currentThreadId() << std::endl;
    int numMessages = 0;
    while(true)
    {
        mutexmain->lock();
        numMessages = messagesStorage->size();
        mutexmain->unlock();

        if ( numMessages > RECORDSIZE || forceWrite )
        {
            QDateTime current = QDateTime::currentDateTime();
            QString timeStamp = current.toString( QLatin1String("yyyyMMdd_hhmmss") );

            std::cout << "Write file to: " << filesrc.arg( timeStamp ).toStdString() << std::endl;
            // std::cout << "messagesStorage->length() in loop" << messagesStorage->size() << " vs. " << numMessages << std::endl;

            if ( forceWrite )
            {
                numMessages = (RECORDSIZE < numMessages) ? RECORDSIZE : numMessages; //changed by theSeim
                forceWrite = false;
            }

            // write to file    
            QFile data( filesrc.arg( timeStamp ));
            if ( data.open( QFile::WriteOnly))
            {
                QTextStream outStream( &data );
                for ( int i=0; i<numMessages; i++ )
                {
                    // std::cout << "Read entry " << i<<  std::endl;
                    QStringList msg = messagesStorage->at(i);
                    // QString v = QString::number(filecount*RECORDSIZE+i+1) + ": " + msg.join( "|") + "\n";
                    QString v = msg.join( "|") + "\n";
                    outStream << v;
                }

                data.close();
                filecount++;
            }
            else
            {
                // TODO handle
            }

            // block thread
            mutexmain->lock();
            // std::cout << "Remove entries " << std::endl;
            for ( int i=0; i<RECORDSIZE; i++ )
            {
                if ( !messagesStorage->isEmpty() )
                {
                    messagesStorage->removeFirst();
                }
            }
            // unblock thread
            mutexmain->unlock();

            // std::cout << "New messageStoreag Length: " << messagesStorage->length() << std::endl;

        }
    }

}



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
                numMessages = std::min( RECORDSIZE, numMessages );
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



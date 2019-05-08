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
#include <QCoreApplication>
#include <QtNetwork/QNetworkInterface>
#include <QtNetwork/QHostAddress>
#include <iostream>
#include <mainapp.h>
#include <regex>
#include <QStringList>

int main(int argc, char *argv[])
{
    QCoreApplication a(argc, argv);

    QStringList cmdlineArgs = QCoreApplication::arguments();

    if(cmdlineArgs.length() <= 1)
    {
        std::cout << "-h, -help:            display this help" << std::endl;
        std::cout << "-ownIP:               IP address of this computer" << std::endl;
        std::cout << "-d:                   run with debug output" << std::endl;
#ifdef Q_OS_MACOS
        std::cout << "Note: This SyncServer version does not support NCam input." << std::endl;
#endif
#ifdef Q_OS_WIN
        std::cout << "-ncamIP:              ncam server IP address" << std::endl;
        std::cout << "-ncamPort:            ncam server port, neccessary if not 38860" << std::endl;
#endif
    }
    else
    {
        QList<QHostAddress> availableIpAdresses = QNetworkInterface::allAddresses();
        int i = 0;
        while(i < availableIpAdresses.length())
        {
            if(availableIpAdresses[i].toString().contains(":"))
            {
                availableIpAdresses.removeAt(i);
            }
            else
            {
                i++;
            }
        }

        QString ownIP = "";
        QString ncamIP = "";
        QString ncamPort = "";
        bool debug = false;

        int k = 1;
        while(k < cmdlineArgs.length())
        {
            if(cmdlineArgs[k] == "-d")
            {
                std::cout << "Debug output enabled." << std::endl;
                debug = true;
                k = k+1;
                continue;
            }
            if(cmdlineArgs[k] == "-ownIP")
            {
                foreach(QHostAddress ipAdress, availableIpAdresses)
                {
                    if(ipAdress.toString() == cmdlineArgs[k+1])
                    {
                        ownIP = cmdlineArgs[k+1];
                    }
                }
            }
            else if(cmdlineArgs[k] == "-ncamIP")
            {
#ifdef Q_OS_MACOS
        std::cout << "Note: This SyncServer version does not support NCam input." << std::endl;
#endif
#ifdef Q_OS_WIN
                QHostAddress checkIP;
                if(checkIP.setAddress(cmdlineArgs[k+1]))
                {
                    ncamIP = cmdlineArgs[k+1];
                }
#endif
            }
            else if(cmdlineArgs[k] == "-ncamPort")
            {
#ifdef Q_OS_MACOS
        std::cout << "Note: This SyncServer version does not support NCam input." << std::endl;
#endif
#ifdef Q_OS_WIN
                ncamPort = cmdlineArgs[k+1];
#endif
            }
            k = k+2;
        }

        if(ownIP == "")
        {
            std::cout << "No valid IP address for this server (own IP adress) was defined." << std::endl;
            std::cout << "Choose from the following valid IP adresses of this PC:" << std::endl;
            for(int l = 0; l < availableIpAdresses.length(); l++)
            {
                    std::cout << availableIpAdresses[l].toString().toLatin1().data() << std::endl;
            }
        }
        else
        {
#ifdef Q_OS_MACOS
        std::cout << "Note: This SyncServer version does not support NCam input." << std::endl;
#endif
#ifdef Q_OS_WIN
            if(ncamIP == "")
            {
                std::cout << "No valid IP address for ncam server defined." << std::endl;
            }
            if(ncamPort == "")
            {
                std::cout << "No new port for ncam server defined. Use 38860." << std::endl;
                ncamPort = "38860";
            }
#endif
            MainApp* app = new MainApp(ownIP, ncamIP, ncamPort, debug);
            app->run();

            return a.exec();
        }
    }
}

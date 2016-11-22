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
        std::cout << "-ncamIP:              ncam server IP address" << std::endl;
        std::cout << "-ncamPort:            ncam server port, neccessary if not 38860" << std::endl;
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

        int k = 1;
        while(k < cmdlineArgs.length()-1)
        {
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
                QHostAddress checkIP;
                if(checkIP.setAddress(cmdlineArgs[k+1]))
                {
                    ncamIP = cmdlineArgs[k+1];
                }
            }
            else if(cmdlineArgs[k] == "-ncamPort")
            {
                ncamPort = cmdlineArgs[k+1];
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
            if(ncamIP == "")
            {
                std::cout << "No valid IP address for ncam server defined." << std::endl;
            }
            if(ncamPort == "")
            {
                std::cout << "No new port for ncam server defined. Use 38860." << std::endl;
                ncamPort = "38860";
            }
            MainApp* app = new MainApp(ownIP, ncamIP, ncamPort);
            app->run();

            return a.exec();
        }
    }
}

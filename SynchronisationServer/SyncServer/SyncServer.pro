QT += core network
QT -= gui

CONFIG += c++11

TARGET = SyncServer
CONFIG += console
CONFIG -= app_bundle

TEMPLATE = app

SOURCES += \
    ../src/zeroMQHandler.cpp \
    ../src/transformationrecorder.cpp \
    ../src/recordWriter.cpp \
    ../src/printer.cpp \
    ../src/ncamadapter.cpp \
    ../src/mainapp.cpp \
    ../src/main.cpp

HEADERS += \
    ../include/zeroMQHandler.h \
    ../include/transformationrecorder.h \
    ../include/recordWriter.h \
    ../include/printer.h \
    ../include/ncamsimpleclient.h \
    ../include/ncamadapter.h \
    ../include/mainapp.h

INCLUDEPATH += ../include \
            ../nzmqt/include \
            ../nzmqt/3rdparty/cppzmq \
            ../zeromq-3.2.4/include \
            ../ncam-2.6/include

LIBS += -L/opt/katana2.1v4/bin/ZeroMQ/lib -lzmq \
        -L/Dreamspace/VPET/SynchronisationServer/ncam-2.6/lib/Linux/GCC\ 4.8.4\ 64bits/Release -lLibNcamDataStreaming

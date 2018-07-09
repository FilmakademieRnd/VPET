QT += core network
QT -= gui

CONFIG += c++11

TARGET = SyncServer
CONFIG += console
CONFIG -= app_bundle

TEMPLATE = app

SOURCES += \
    ../src/objectStateHandler.cpp \
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
    ../include/mainapp.h \
    ../include/objectStateHandler.h

INCLUDEPATH += ../include \
            ../zeromq-4.2.3/include \
            ../nzmqt/include \
            ../nzmqt/3rdparty/cppzmq \
            ../ncam-2.6/include

LIBS += -L../zeromq-4.2.3/bin/x64/Release/v140/dynamic -llibzmq \
        -L../ncam-2.6/lib/Windows/MSVC2013_MT/64bits -lLibNcamDataStreaming

QT += core network
QT -= gui

CONFIG += c++11

TARGET = SyncServer
CONFIG += console


SOURCES += \
    ../src/zeroMQHandler.cpp \
    ../src/transformationrecorder.cpp \
    ../src/recordWriter.cpp \
    ../src/printer.cpp \
    ../src/mainapp.cpp \
    ../src/objectStateHandler.cpp \
    ../src/main.cpp



win32{
    SOURCES += \
        ../src/ncamadapter.cpp
}

HEADERS += \
    ../include/zeroMQHandler.h \
    ../include/transformationrecorder.h \
    ../include/recordWriter.h \
    ../include/printer.h \
    ../include/mainapp.h \
    ../include/objectStateHandler.h

win32{
    HEADERS += \
        ../include/ncamsimpleclient.h \
        ../include/ncamadapter.h
}

win32{
    INCLUDEPATH += ../include \
                ../zeromq-4.2.3/include \
                ../nzmqt/include \
                ../nzmqt/3rdparty/cppzmq \
                ../ncam-2.6/include

    LIBS += -L../zeromq-4.2.3/bin/x64/Release/v140/dynamic -llibzmq \
            -L../ncam-2.6/lib/Windows/MSVC2013_MT/64bits -lLibNcamDataStreaming
}

macx{
    INCLUDEPATH += ../include \
                $$PWD/../../../../../usr/local/Cellar/zeromq/4.2.5/include \
                ../nzmqt/include \
                ../nzmqt/3rdparty/cppzmq \

    LIBS += -L$$PWD/../../../../../usr/local/Cellar/zeromq/4.2.5/lib/ -lzmq

    include(../nzmqt-master/nzmqt.pri)
}

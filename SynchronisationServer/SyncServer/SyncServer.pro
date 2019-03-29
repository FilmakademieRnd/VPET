QT += core network
QT -= gui

CONFIG += c++11

TARGET = SyncServer
CONFIG += console

TEMPLATE = app


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

win32{
    INCLUDEPATH += ../include \
                ../zeromq-4.3.1/build/install/include \
                ../nzmqt/include \
                ../nzmqt/3rdparty/cppzmq \
                ../ncam-2.6/include

    LIBS += -L../zeromq-4.3.1/build/install/lib -llibzmq-v141-mt-4_3_1 \
            -L../ncam-2.6/lib/Windows/MSVC2013_MT/32bits -lLibNcamDataStreaming
}

macx{
    INCLUDEPATH += ../include \
                $$PWD/../../../../../usr/local/Cellar/zeromq/4.2.5/include \
                ../nzmqt/include \
                ../nzmqt/3rdparty/cppzmq \

    LIBS += -L$$PWD/../../../../../usr/local/Cellar/zeromq/4.2.5/lib/ -lzmq

    include(../nzmqt-master/nzmqt.pri)
}

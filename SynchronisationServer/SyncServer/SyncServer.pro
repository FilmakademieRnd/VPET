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


SOURCES += \
    ../src/zeroMQHandler.cpp \
    ../src/transformationrecorder.cpp \
    ../src/recordWriter.cpp \
    ../src/printer.cpp \
    ../src/mainapp.cpp \
    ../src/objectStateHandler.cpp \
    ../src/main.cpp


win32{
    INCLUDEPATH += ../include \
                ../zeromq-4.3.1/build/install/include \
                ../nzmqt/include \
                ../nzmqt/3rdparty/cppzmq

    LIBS += -L../zeromq-4.3.1/build/install/lib -llibzmq-v141-mt-4_3_1
}

macx{
    INCLUDEPATH += ../include \
                ../../../../../../../../../usr/local/Cellar/zeromq/4.3.2/include \
                ../../../nzmqt/include \
                ../../../nzmqt/3rdparty/cppzmq \

    LIBS += -L../../../../../../../../../usr/local/Cellar/zeromq/4.3.2/lib/ -lzmq

    include(../../../nzmqt/nzmqt.pri)
}

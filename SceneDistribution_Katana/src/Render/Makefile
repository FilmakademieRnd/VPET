# The output objects dir
OBJDIR = ./out

# The plugin .so file
OUTFILENAME=SceneDistributorPlugin.so
OUTFILEPATH = $(OBJDIR)/$(OUTFILENAME)

# Plugin sources and includes
SOURCES +=  src/SceneDistributorPlugin.cpp
SOURCES +=  src/SceneIterator.cpp

INCLUDES = -Iinclude
INCLUDES += -I$(KATANA_HOME)/plugin_apis/include

# Third Party libraries
ifdef ZMQ_HOME
INCLUDES += -I$(ZMQ_HOME)/include
endif

ifdef ZMQ_HOME
LIBPATH += -L$(ZMQ_HOME)/lib
endif
LIBPATH += -L/usr/lib64

ifdef ZMQ_HOME
#LIBS += -lzmq_static
endif
#LIBS += -lHalf -lIex -lIlmImf -lIlmThread -lImath
LIBS += -Wl,-Bdynamic
ifdef ZMQ_HOME
LIBS += -lzmq
endif

# PLUGIN APIs sources and includes
PLUGIN_SRC = $(KATANA_HOME)/plugin_apis/src

# Base Foundry Plugin API
SOURCES += $(PLUGIN_SRC)/FnPluginSystem/FnPlugin.cpp

# Render Plugin API: Base
SOURCES += $(PLUGIN_SRC)/FnRender/plugin/RenderBase.cpp
SOURCES += $(PLUGIN_SRC)/FnRender/plugin/CameraSettings.cpp
SOURCES += $(PLUGIN_SRC)/FnRender/plugin/DiskRenderOutputProcess.cpp
SOURCES += $(PLUGIN_SRC)/FnRender/plugin/GlobalSettings.cpp
SOURCES += $(PLUGIN_SRC)/FnRender/plugin/RenderSettings.cpp

# Render Plugin API: Render Actions
SOURCES += $(PLUGIN_SRC)/FnRender/plugin/RenderAction.cpp
SOURCES += $(PLUGIN_SRC)/FnRender/plugin/NoOutputRenderAction.cpp
SOURCES += $(PLUGIN_SRC)/FnRender/plugin/TemporaryRenderAction.cpp
SOURCES += $(PLUGIN_SRC)/FnRender/plugin/CopyRenderAction.cpp
SOURCES += $(PLUGIN_SRC)/FnRender/plugin/CopyAndConvertRenderAction.cpp

# Render Plugin API: Interprocess Communication
SOURCES += $(PLUGIN_SRC)/FnRender/plugin/IdSenderFactory.cpp
SOURCES += $(PLUGIN_SRC)/FnRender/plugin/SocketConnection.cpp
SOURCES += $(PLUGIN_SRC)/FnRender/plugin/SocketIdSender.cpp

# Renderer Info API
SOURCES += $(PLUGIN_SRC)/FnRendererInfo/plugin/RenderMethod.cpp

# Render Output Utilities API
SOURCES += $(PLUGIN_SRC)/FnRenderOutputUtils/client/FnRenderOutputUtils.cpp
SOURCES += $(PLUGIN_SRC)/FnGeolibServices/client/FnArbitraryOutputAttr.cpp
SOURCES += $(PLUGIN_SRC)/FnGeolibServices/client/FnXFormUtil.cpp
SOURCES += $(PLUGIN_SRC)/FnRenderOutputUtils/client/CameraInfo.cpp
SOURCES += $(PLUGIN_SRC)/FnRenderOutputUtils/client/ShadingNodeConnectionDescription.cpp
SOURCES += $(PLUGIN_SRC)/FnRenderOutputUtils/client/ShadingNodeDescription.cpp
SOURCES += $(PLUGIN_SRC)/FnRenderOutputUtils/client/ShadingNodeDescriptionMap.cpp
SOURCES += $(PLUGIN_SRC)/FnRenderOutputUtils/client/XFormMatrix.cpp

#ifdef ZMQ_HOME
# Display Driver API (requires ZeroMQ)
#SOURCES += $(PLUGIN_SRC)/FnDisplayDriver/ContextSingleton.cpp
#SOURCES += $(PLUGIN_SRC)/FnDisplayDriver/CloseMessage.cpp
#SOURCES += $(PLUGIN_SRC)/FnDisplayDriver/DataMessage.cpp
#SOURCES += $(PLUGIN_SRC)/FnDisplayDriver/KatanaPipe.cpp
#SOURCES += $(PLUGIN_SRC)/FnDisplayDriver/KatanaPipeSingleton.cpp
#SOURCES += $(PLUGIN_SRC)/FnDisplayDriver/Message.cpp
#SOURCES += $(PLUGIN_SRC)/FnDisplayDriver/NewChannelMessage.cpp
#SOURCES += $(PLUGIN_SRC)/FnDisplayDriver/NewFrameMessage.cpp
#SOURCES += $(PLUGIN_SRC)/FnDisplayDriver/ZmqHelpers.cpp
#SOURCES += $(PLUGIN_SRC)/FnDisplayDriver/IsAliveMessage.cpp
#SOURCES += $(PLUGIN_SRC)/FnDisplayDriver/EndOfRenderMessage.cpp
#endif

# Asset API
SOURCES += $(PLUGIN_SRC)/FnAsset/plugin/FnAsset.cpp
SOURCES += $(PLUGIN_SRC)/FnAsset/plugin/FnFileSequence.cpp
SOURCES += $(PLUGIN_SRC)/FnAsset/client/FnDefaultAssetPlugin.cpp
SOURCES += $(PLUGIN_SRC)/FnAsset/client/FnDefaultFileSequencePlugin.cpp

# Attribute API
SOURCES += $(PLUGIN_SRC)/FnAttribute/client/FnAttribute.cpp
SOURCES += $(PLUGIN_SRC)/FnAttribute/client/FnDataBuilder.cpp

# Logging API
SOURCES += $(PLUGIN_SRC)/FnLogging/client/FnLogging.cpp

# Scenegraph Iterator API
SOURCES += $(PLUGIN_SRC)/FnScenegraphIterator/client/FnScenegraphIterator.cpp

# Plugin API
SOURCES += $(PLUGIN_SRC)/FnPluginManager/client/FnPluginManager.cpp

# Object files and flags
OBJS := $(patsubst %.cpp,$(OBJDIR)/%.o,$(SOURCES))

CXXFLAGS = -Wall -pipe -m64 -fPIC -DPIC -lpthread

# Targets:
all: $(OUTFILEPATH)

$(OUTFILEPATH): $(OBJS)
	@echo "  Compiling OpenRL Render plugin..."
	$(CXX) $(CXXFLAGS) $(OBJS)  $(LIBPATH) $(LIBS) -shared -o $(OUTFILEPATH) -Wl,-soname,$(OUTFILENAME)


$(OBJDIR)/%.o: %.cpp
	@mkdir -p `dirname $@`
	$(CXX) $(CXXFLAGS) $(INCLUDES) -c $< -o $@


clean:
	@echo "  Cleaning OpenRL Render plugin"
	@rm -rf $(OBJDIR)

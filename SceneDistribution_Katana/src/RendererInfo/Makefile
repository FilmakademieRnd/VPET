# The output objects dir
OBJDIR = ./out

# The plugin .so file
OUTFILENAME = SceneDistributorInfoPlugin.so
OUTFILEPATH = $(OBJDIR)/$(OUTFILENAME)

# Plugin sources and includes
SOURCES = src/SceneDistributorInfoPlugin.cpp

INCLUDES = -Iinclude

# Third Party libraries
LIBS += -ldl

# PLUGIN APIs sources and includes
PLUGIN_SRC = $(KATANA_HOME)/plugin_apis/src

# Base Foundry Plugin API
SOURCES += $(PLUGIN_SRC)/FnPluginSystem/FnPlugin.cpp

# Renderer Info API
SOURCES += $(PLUGIN_SRC)/FnRendererInfo/plugin/RendererInfoBase.cpp
SOURCES += $(PLUGIN_SRC)/FnRendererInfo/plugin/LiveRenderControlModule.cpp
SOURCES += $(PLUGIN_SRC)/FnRendererInfo/plugin/LiveRenderFilter.cpp
SOURCES += $(PLUGIN_SRC)/FnRendererInfo/plugin/RenderMethod.cpp
SOURCES += $(PLUGIN_SRC)/FnRendererInfo/plugin/ShaderInfoCache.cpp

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

INCLUDES += -I$(KATANA_HOME)/plugin_apis/include

# Object files and flags
OBJS := $(patsubst %.cpp,$(OBJDIR)/%.o,$(SOURCES))

CXXFLAGS = -Wall -pipe -m64 -fPIC -DPIC

# Targets:
all: $(OUTFILEPATH)

$(OUTFILEPATH): $(OBJS)
	@echo "  Compiling SceneDistributor RendererInfo plugin..."
	$(CXX) $(CXXFLAGS) $(OBJS) $(LIBPATH) $(LIBS) -shared -o $(OUTFILEPATH)

$(OBJDIR)/%.o: %.cpp
	@mkdir -p `dirname $@`
	$(CXX) $(CXXFLAGS) $(INCLUDES) -c $< -o $@

clean:
	@echo "  Cleaning SceneDistributor RendererInfo plugin"
	@rm -rf $(OBJDIR)

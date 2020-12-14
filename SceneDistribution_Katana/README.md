# Katana + VPET


## New Features:
- **env&#46;sh**  
	This version should be compiled against KatanaPlayer.  
	See changes to the KATANA_HOME environment variable to reflect this.
- **RendererSettings.xml**  
	Modified configfile to include OpenRL buffer requirements.  
	This must be passed as the -configfile argument for KatanaPlayer.
- **OpenRLPlugin/src/Render/include/PluginState.h**  
	Using the BufferManager API to hold information about allocated memory in KatanaPlayer to display rendered images.
- **OpenRLPlugin/src/Render/src/OpenRLRenderPlugin.cpp**  
	The constructor initializes handles to the BufferManager to allocate memory to display rendered images.  
	The start method creates a dummy image block and writes to the buffer allocated.


## Dependencies:
- **KatanaPlayer**: provides the API for building render plugins.
- **Boost**: required by the API to build render plugins for Katana/KatanaPlayer.
- **ZeroMQ** (optional): required by the API only if building display drivers for Katana/KatanaPlayer.
- **OpenEXR** (optional): used in the OpenRL render plugin to easily manipulate and print matrices and vectors.


## Contents:
- **env&#46;sh**   
	Example of how to set up the required dependencies and environment variables for building.  
	Tested dependency versions inside.
- **RendererSettings.xml**  
	Modified configfile to include OpenRL buffer requirements.
- **OpenRLPlugin**  
	The KatanaPlayer render plugin source code and makefile to build all necessary plugins.  
	The code is derived from Katana's render plugin template: $KATANA_HOME/plugins/Src/Templates
- **OpenRLPlugin/src/RendererInfo**  
	Renderer information to setup scenes in Katana, like available shaders or render AOVs.  
	This also includes terminal Ops: used to observe scene changes when Live Rendering.
- **OpenRLPlugin/src/Render**  
	The MAIN plugin to traverse the scene graph, to extract the scene data, and to control the renderer.
- **OpenRLPlugin/src/Render/src/OpenRLRenderPlugin.cpp**  
	OpenRL's main render plugin called when a render start, or when receiving updates during a live rendering session.  
	This also allocates a buffer to write rendered images to KatanaPlayer.
- **OpenRLPlugin/src/ScenegraphLocationDelegate**  
	Additional helpers to process specific scene graph types.  
	It is possible to not use any delegates, instead the whole scene graph has to be processed in the Render plugin.
- **Resources**  
	This path should be added to the KATANA_RESOURCES environment variable so that KatanaPlayer finds the plugins at runtime.
- **Resources/Libs**  
	The plugin libraries that are loaded as a resource by KatanaPlayer.  
	Links to the expected path of the built plugins are provided for convenience.


## Build:
```
# edit env.sh and adjust environment variables
$ source env.sh
$ cd OpenRLPlugin
$ make
$ cd ..
# check if the links are correctly pointing to the built plugins
$ ls -AlF ./Resources/Libs
```


## Run:
```
# Full path to where KatanaPlayer was installed
export KATANA_PLAYER_HOME=/opt/Foundry/KatanaPlayer1.0v1a1

# Full path to this folder
export OPENRL_PLUGIN_HOME=$PWD
# Add this render plugin to the resources
export KATANA_RESOURCES=$KATANA_RESOURCES:$OPENRL_PLUGIN_HOME/Resources

# Launch KatanaPlayer
$KATANA_PLAYER_HOME/katanaPlayer \
-katanaRoot $KATANA_PLAYER_HOME \
-tempDir /tmp \
-geolib3OpTree /path/to/scene.optree \
-renderMethodType liveRender \
-renderMethodName liveRender \
-configfile $OPENRL_PLUGIN_HOME/RendererSettings.xml \
-host $HOSTNAME:15900
```


## License

Please review the [License file](LICENSE.TXT).
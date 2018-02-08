cmake_minimum_required (VERSION 3.2)

add_library(ScenegraphLocationDelegate1 MODULE
    src/CameraScenegraphLocationDelegate.cpp
    src/GeometryScenegraphLocationDelegate.cpp
    src/LightScenegraphLocationDelegate.cpp
    src/registry.cpp
)

add_definitions(-DFNVIEWER_STATIC=1 -D_USE_MATH_DEFINES -DFNLOGGING_STATIC)

target_include_directories(ScenegraphLocationDelegate1
    PRIVATE
        include
        ${OPENGL_INCLUDE_DIR}
		${GLM_INCLUDE_DIRS}
        ${KATANA_HOME}/plugin_apis/include
		../Render/include
)
target_link_libraries(ScenegraphLocationDelegate1
    PRIVATE
        Katana::FnAttribute
        Katana::FnPluginSystem
        Katana::FnScenegraphIterator
        Katana::FnRenderPlugin
        Katana::FnRenderOutputUtils
        Katana::FnGeolibServices
        Katana::FnPluginSystem
        Katana::FnLogging
        Katana::FnAsset
        Katana::pystring
        Katana::FnPluginManager
        ${OPENGL_LIBRARIES}
        ${GLM_LIBRARIES})

set_target_properties(ScenegraphLocationDelegate1 PROPERTIES PREFIX "")
install(TARGETS ScenegraphLocationDelegate1 DESTINATION Libs)

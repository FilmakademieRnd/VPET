cmake_minimum_required (VERSION 3.2)
project(SceneDistributor)

set(KATANA_HOME "${CMAKE_CURRENT_LIST_DIR}/../../../.." CACHE PATH "Path to Katana")
list(INSERT CMAKE_MODULE_PATH 0 "${KATANA_HOME}/plugins/Src/cmake")

if (MSVC)
    set(PTHREAD_HOME "${CMAKE_CURRENT_LIST_DIR}" CACHE PATH "Path to PThread Dir")
	set(PTHREAD_LIBRARIES "${CMAKE_CURRENT_LIST_DIR}" CACHE FILEPATH "Path to PThread library")
endif ()

find_package(Katana PATHS "${KATANA_HOME}/plugin_apis/cmake" REQUIRED)
find_package(OpenGL)
find_package(GLEW)
find_package(glm)
find_package(ZeroMQ)

set (CMAKE_CXX_STANDARD 11)

if (NOT MSVC)
    add_compile_options(-Wall)
endif ()

# Ensure we don't compile DSOs with undefined symbols.
if (CMAKE_SYSTEM_NAME MATCHES Linux)
    set(CMAKE_SHARED_LINKER_FLAGS "${CMAKE_SHARED_LINKER_FLAGS} -Wl,--no-undefined")
    set(CMAKE_MODULE_LINKER_FLAGS "${CMAKE_MODULE_LINKER_FLAGS} -Wl,--no-undefined")
endif ()

if (NOT OPENGL_FOUND)
    message(WARNING "Not compiling Scene Distributor plugins as OpenGL was not found.")
    return ()
endif ()

if (NOT GLEW_FOUND)
    message(WARNING "Not compiling Scene Distributor plugins as GLEW was not found.")
    return ()
endif ()

if (NOT glm_FOUND)
    message(WARNING "Not compiling Scene Distributor plugins as GLM was not found.")
    return ()
endif ()

if (NOT ZeroMQ_FOUND)
    message(WARNING "Not compiling Scene Distributor plugins as ZeroMQ was not found.")
    return ()
endif ()

add_subdirectory(Render)
add_subdirectory(RendererInfo)
add_subdirectory(ScenegraphLocationDelegate)

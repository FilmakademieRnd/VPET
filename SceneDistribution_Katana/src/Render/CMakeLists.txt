add_library(SceneDistributorPlugin MODULE 
	src/SceneDistributorPlugin
	src/SceneIterator.cpp)

target_include_directories(SceneDistributorPlugin 
	PRIVATE 
		${GLM_INCLUDE_DIRS}
		${PTHREAD_HOME}/include
		${ZeroMQ_INCLUDE_DIR}
		include)

target_link_libraries(SceneDistributorPlugin
    PRIVATE
    Katana::FnRenderOutputUtils
    Katana::FnRenderPlugin
    Katana::FnScenegraphIterator
    Katana::FnGeolibServices
	${PTHREAD_LIBRARIES}
	${ZeroMQ_LIBRARY}
)

set_target_properties(SceneDistributorPlugin PROPERTIES PREFIX "")
install(TARGETS SceneDistributorPlugin DESTINATION Libs)

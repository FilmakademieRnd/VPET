add_library(SceneDistributorInfoPlugin MODULE src/SceneDistributorInfoPlugin)

target_include_directories(SceneDistributorInfoPlugin PRIVATE include)

target_link_libraries(SceneDistributorInfoPlugin
    PRIVATE
    Katana::FnRendererInfoPlugin
)

set_target_properties(SceneDistributorInfoPlugin PROPERTIES PREFIX "")
install(TARGETS SceneDistributorInfoPlugin DESTINATION Libs)

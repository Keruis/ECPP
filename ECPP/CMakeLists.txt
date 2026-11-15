add_subdirectory(lib)
add_subdirectory(ecpp_lang)

add_library(ecpp INTERFACE)

add_library(ECPP::ECPP ALIAS ecpp)

target_link_libraries(ecpp INTERFACE
    ECPP::lang
)

if(BUILD_TESTING)
    add_subdirectory(test)
endif()
add_library(ecpp_lang STATIC)
add_library(ECPP::lang ALIAS ecpp_lang)

add_subdirectory(src)

target_include_directories(ecpp_lang PUBLIC
    "${CMAKE_CURRENT_SOURCE_DIR}/include"
)

target_link_libraries(ecpp_lang PUBLIC
    ECPP::lib
)

target_compile_features(ecpp_lang PUBLIC
    cxx_std_23
)
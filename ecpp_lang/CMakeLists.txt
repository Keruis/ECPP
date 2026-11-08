add_library(ecpp_lang STATIC)

add_library(ECPP::lang ALIAS ecpp_lang)

add_subdirectory(src/lex)
add_subdirectory(src/parser)

target_compile_features(ecpp_lang PUBLIC
    cxx_std_23
)

target_include_directories(ecpp_lang
    PUBLIC
        "$<BUILD_INTERFACE:${CMAKE_CURRENT_SOURCE_DIR}/include>"

    PRIVATE
        "${CMAKE_CURRENT_SOURCE_DIR}/../config"
)

target_link_libraries(ecpp_lang PUBLIC
    ECPP::lib
)
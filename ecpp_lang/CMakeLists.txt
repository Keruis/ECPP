add_subdirectory(src/lex)
add_subdirectory(src/parser)

add_library(ecpp_lang STATIC
    ${LEX_SOURCES}
    ${PARSER_SOURCES}
)

target_compile_features(ecpp_lang PRIVATE cxx_std_23)

target_include_directories(ecpp_lang 
PUBLIC
    "$<BUILD_INTERFACE:${CMAKE_CURRENT_SOURCE_DIR}/include>"
    "${PROJECT_SOURCE_DIR}/config"

PRIVATE
    "$<BUILD_INTERFACE:${CMAKE_CURRENT_SOURCE_DIR}/src>"
)

target_link_libraries(ecpp_lang PRIVATE
    ecpp_lib
)
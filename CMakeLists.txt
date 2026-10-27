cmake_minimum_required(VERSION 3.23)

project(ECPP LANGUAGES CXX)

set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

add_subdirectory(utility)
add_subdirectory(support)
add_subdirectory(ADT)
add_subdirectory(basic)
add_subdirectory(parser)

add_executable(ECPP
    ${PARSER_SOURCES}
    ECPP.cpp
)

target_compile_features(ECPP PRIVATE cxx_std_23)

target_include_directories(ECPP PRIVATE
    ${UTILITY_DIR}
    ${SUPPORT_DIR}
    ${ADT_DIR}
    ${BASIC_DIR}
    ${PARSER_INCLUDE_DIR}
)
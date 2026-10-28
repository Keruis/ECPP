cmake_minimum_required(VERSION 3.23)

project(ECPP LANGUAGES CXX)

set(CMAKE_EXPORT_COMPILE_COMMANDS ON)

add_subdirectory(lib)
add_subdirectory(ecpp_lang)

add_executable(ECPP
    ECPP.cpp
)

target_compile_features(ECPP PRIVATE cxx_std_23)

target_link_libraries(ECPP PRIVATE
    ecpp_lib
    ecpp_lang
)
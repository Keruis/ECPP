cmake_minimum_required(VERSION 3.23)

project(ECPP LANGUAGES CXX)

set(CMAKE_EXPORT_COMPILE_COMMANDS ON CACHE BOOL
    "Generate compile_commands.json"
)

add_subdirectory(lib)
add_subdirectory(ecpp_lang)

add_executable(ECPP
    ECPP.cpp
)

target_link_libraries(ECPP PRIVATE
    ECPP::lang
)
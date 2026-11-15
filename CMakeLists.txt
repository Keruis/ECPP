cmake_minimum_required(VERSION 3.23)

project(ECPP LANGUAGES CXX)

set(CMAKE_EXPORT_COMPILE_COMMANDS ON CACHE BOOL
    "Generate compile_commands.json"
)

add_subdirectory(ECPP)

add_executable(ECPP
    main.cpp
)

set_target_properties(ECPP PROPERTIES
    OUTPUT_NAME "ecpp"
)

target_link_libraries(ECPP PRIVATE
    ecpp
)
add_subdirectory(src/basic)

add_library(ecpp_lib STATIC
    ${BASIC_SOURCES}
)

target_compile_features(ecpp_lib PRIVATE cxx_std_23)

target_include_directories(ecpp_lib PUBLIC
    "${CMAKE_CURRENT_SOURCE_DIR}/include"
)
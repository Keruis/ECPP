add_library(ecpp_lib STATIC)

add_library(ECPP::lib ALIAS ecpp_lib)

add_subdirectory(src/basic)

target_compile_features(ecpp_lib PUBLIC
    cxx_std_23
)

target_include_directories(ecpp_lib PUBLIC
    "$<BUILD_INTERFACE:${CMAKE_CURRENT_SOURCE_DIR}/include>"
)
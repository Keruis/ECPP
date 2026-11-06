#pragma once

#include <string_view>

#include <basic/location.h>

namespace BASIC {

[[noreturn]] void Fail(const std::string_view message, const Location loc);

}
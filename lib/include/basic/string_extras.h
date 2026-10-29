#pragma once

#include <basic/char_extras.h>
#include <cstddef>
#include <utility/integer_extras.h>

#include <cassert>

namespace BASIC {

std::size_t ScanNumeric(const char8_t*, const char8_t*, std::size_t) noexcept;

std::size_t IntegerStringToNumeric(const char8_t*, const char8_t*, bool&, std::size_t, std::size_t) noexcept;

}
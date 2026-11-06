#pragma once

#include <basic/char_extras.h>

#include <concepts>

namespace UTILITY {

constexpr bool WouldOverflow(const std::unsigned_integral auto value, const std::unsigned_integral auto digit, const std::unsigned_integral auto M, const std::size_t radix) noexcept {
    return value > (M - digit) / radix;
}

}
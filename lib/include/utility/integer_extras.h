#pragma once

#include <basic/char_extras.h>

#include <concepts>

namespace UTILITY {

constexpr bool WouldOverflow(std::unsigned_integral auto value, std::unsigned_integral auto digit, std::unsigned_integral auto M, std::size_t radix) noexcept {
    return value > (M - digit) / radix;
}

}
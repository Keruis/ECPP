#pragma once

#include <cstddef>

namespace UTILITY {

template <std::size_t radix>
constexpr bool IsDigit(char c) noexcept {
    if constexpr (radix == 16) {
        return IsDigit<10>(c) || ((c | 0x20) >= 'a' && (c | 0x20) <= 'f');
    } else if (radix == 10) {
        return c >= '0' && c <= '9';
    }
    static_assert("error");
}

}
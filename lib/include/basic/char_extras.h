#pragma once

#include <utility/algorithms.h>

#include <cstddef>

namespace BASIC {

template <std::size_t radix>
constexpr bool IsDigit(char8_t c) noexcept {
    if constexpr (radix == 16) {
        return IsDigit<10>(c) || ((c | 0x20) >= 'a' && (c | 0x20) <= 'f');
    } else if (radix == 10) {
        return c >= '0' && c <= '9';
    } else if (radix == 8) {
        return c >= '0' && c <= '7';
    } else if (radix == 2) {
        return c == '0' || c == '1';
    }
    static_assert("BASIC::IsDigit");
}

constexpr bool IsLowercase(char8_t c) noexcept {
    return c >= 'a' && c <= 'z';
}

constexpr bool IsUppercase(char8_t c) noexcept {
    return c >= 'A' && c <= 'Z';
}

constexpr bool IsLetter(char8_t c) noexcept {
    return IsLowercase(c) || IsUppercase(c);
}

constexpr bool IsAlphanumeric(char8_t c) noexcept {
    return IsLetter(c) || IsDigit<10>(c);
}

constexpr bool IsHorizontalWhitespace(char8_t c) noexcept {
    return UTILITY::IsAnyOf(c, ' ', '\t', '\f', '\v');
}

constexpr bool IsVerticalWhitespace(char8_t c) noexcept {
    return UTILITY::IsAnyOf(c, '\n', '\r');
}



}
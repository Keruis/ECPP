#pragma once

#include <cassert>
#include <cstddef>
#include <utility/algorithms.h>

#include <array>
#include <cstdint>
#include <ranges>
#include <utility>

namespace BASIC {

enum CharFlag {
    HORIZONTAL  = 1,
    VERTICAL    = 1 << 1,
    DIGIT_2     = 1 << 2,
    DIGIT_8     = 1 << 3,
    DIGIT_10    = 1 << 4,
    DIGIT_16    = 1 << 5,
    LOWERCASE   = 1 << 6,
    UPPERCASE   = 1 << 7
};

constexpr std::array<std::uint8_t, 256> CharTable = []{
    using enum CharFlag;
    using Digit_10_Interval = UTILITY::Interval<'0', '9'>;
    using Digit_8_Interval = UTILITY::Interval<'0', '7'>;
    using Digit_16_Interval = UTILITY::Interval<'a', 'f'>;
    using Lowercase_Interval = UTILITY::Interval<'a', 'z'>;
    using Uppercase_Interval = UTILITY::Interval<'A', 'Z'>;

    std::array<std::uint8_t, 256> table{};

    for (auto [i, c] : table | std::views::enumerate) {
        if (UTILITY::IsAnyOf(i, ' ', '\t', '\f', '\v'))
            c |= HORIZONTAL;
        if (UTILITY::IsAnyOf(i, '\n', '\r'))
            c |= VERTICAL;
        if (UTILITY::IsAnyOf(i, '0', '1'))
            c |= DIGIT_2;
        if (Digit_8_Interval::IsInClosedInterval(i))
            c |= DIGIT_8;
        if (Digit_10_Interval::IsInClosedInterval(i))
            c |= DIGIT_10;
        if (Digit_10_Interval::IsInClosedInterval(i) || Digit_16_Interval::IsInClosedInterval(i | 0x20))
            c |= DIGIT_16;
        if (Lowercase_Interval::IsInClosedInterval(i))
            c |= LOWERCASE;
        if (Uppercase_Interval::IsInClosedInterval(i))
            c |= UPPERCASE;
    }

    return table;
}();

constexpr bool IsDigit(char32_t c, std::size_t radix) noexcept {
    using enum CharFlag;

    assert(UTILITY::IsAnyOf(radix, 2, 8, 10, 16));

    switch (radix) {
        case 2:
            return UTILITY::HasAnyFlag<CharTable, CharFlag::DIGIT_2>(static_cast<char8_t>(c));
        case 8:
            return UTILITY::HasAnyFlag<CharTable, CharFlag::DIGIT_8>(static_cast<char8_t>(c));
        case 10:
            return UTILITY::HasAnyFlag<CharTable, CharFlag::DIGIT_10>(static_cast<char8_t>(c));
        case 16:
            return UTILITY::HasAnyFlag<CharTable, CharFlag::DIGIT_16>(static_cast<char8_t>(c));
    }

    std::unreachable();
}

constexpr bool IsAscii(char32_t c) noexcept {
    return c <= 127;
}

constexpr bool IsLowercase(char32_t c) noexcept {
    return UTILITY::HasAnyFlag<CharTable, CharFlag::LOWERCASE>(static_cast<char8_t>(c));
}

constexpr bool IsUppercase(char32_t c) noexcept {
    return UTILITY::HasAnyFlag<CharTable, CharFlag::UPPERCASE>(static_cast<char8_t>(c));
}

constexpr bool IsLetter(char32_t c) noexcept {
    return UTILITY::HasAnyFlag<CharTable, CharFlag::LOWERCASE, CharFlag::UPPERCASE>(static_cast<char8_t>(c));
}

constexpr bool IsAlphanumeric(char32_t c) noexcept {
    return UTILITY::HasAnyFlag<CharTable, CharFlag::LOWERCASE, CharFlag::UPPERCASE, CharFlag::DIGIT_10>(static_cast<char8_t>(c));
}

constexpr bool IsHorizontalWhitespace(char32_t c) noexcept {
    return UTILITY::HasAnyFlag<CharTable, CharFlag::HORIZONTAL>(static_cast<char8_t>(c));
}

constexpr bool IsVerticalWhitespace(char32_t c) noexcept {
    return UTILITY::HasAnyFlag<CharTable, CharFlag::VERTICAL>(static_cast<char8_t>(c));
}

constexpr std::uint8_t CharToDigit(char32_t c, std::size_t radix) noexcept {
    assert(IsDigit(c, radix));

    return radix >= 10 
        ? (c <= U'9' ? c - U'0' : (c | 0x20) - U'a' + 10)
        : c - U'0' ;
}

}
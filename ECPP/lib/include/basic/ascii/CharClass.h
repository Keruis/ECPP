#pragma once

#include <utility/Predicate.h>
#include <utility/FlagTable.h>

#include <cassert>
#include <cstdint>
#include <ranges>
#include <utility>

namespace BASIC::ASCII {

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

inline constexpr std::array<std::uint8_t, 256> CharFlagTable = []{
    using enum CharFlag;

    std::array<std::uint8_t, 256> table{};

    for (auto [i, c] : table | std::views::enumerate) {
        if (UTILITY::IsAnyOf(i, ' ', '\t', '\f', '\v'))
            c |= HORIZONTAL;
        if (UTILITY::IsAnyOf(i, '\n', '\r'))
            c |= VERTICAL;
        if (UTILITY::IsAnyOf(i, '0', '1'))
            c |= DIGIT_2;
        if (i >= '0' && i <= '7')
            c |= DIGIT_8;
        if (i >= '0' && i <= '9')
            c |= DIGIT_10;
        if ((i >= '0' && i <= '9') || ((i | 0x20) >= 'a' && (i | 0x20) <= 'f'))
            c |= DIGIT_16;
        if (i >= 'a' && i <= 'z')
            c |= LOWERCASE;
        if (i >= 'A' && i <= 'Z')
            c |= UPPERCASE;
    }

    return table;
}();

template <CharFlag... Flags>
constexpr bool HasASCIIFlag(const char32_t cp) noexcept {
    if (cp > 0x7F)
        return false;

    return UTILITY::HasAnyFlag<CharFlagTable, Flags...>(cp);
}

constexpr bool IsDigit2(const char32_t c) noexcept {
    return HasASCIIFlag<CharFlag::DIGIT_2>(c);
}

constexpr bool IsDigit8(const char32_t c) noexcept {
    return HasASCIIFlag<CharFlag::DIGIT_8>(c);
}

constexpr bool IsDigit10(const char32_t c) noexcept {
    return HasASCIIFlag<CharFlag::DIGIT_10>(c);
}

constexpr bool IsDigit16(const char32_t c) noexcept {
    return HasASCIIFlag<CharFlag::DIGIT_16>(c);
}

constexpr bool IsDigit(const char32_t c, const std::size_t radix) noexcept {
    switch (radix) {
        case 2:
            return IsDigit2(c);

        case 8:
            return IsDigit8(c);

        case 10:
            return IsDigit10(c);

        case 16:
            return IsDigit16(c);
    }

    std::unreachable();
}

constexpr bool IsLowercase(const char32_t c) noexcept {
    return HasASCIIFlag<CharFlag::LOWERCASE>(c);
}

constexpr bool IsUppercase(const char32_t c) noexcept {
    return HasASCIIFlag<CharFlag::UPPERCASE>(c);
}

constexpr bool IsLetter(const char32_t c) noexcept {
    return HasASCIIFlag<CharFlag::LOWERCASE, CharFlag::UPPERCASE>(c);
}

constexpr bool IsAlphanumeric(const char32_t c) noexcept {
    return HasASCIIFlag<CharFlag::LOWERCASE, CharFlag::UPPERCASE, CharFlag::DIGIT_10>(c);
}

constexpr bool IsHorizontalWhitespace(const char32_t c) noexcept {
    return HasASCIIFlag<CharFlag::HORIZONTAL>(c);
}

constexpr bool IsVerticalWhitespace(const char32_t c) noexcept {
    return HasASCIIFlag<CharFlag::VERTICAL>(c);
}

constexpr std::uint8_t Digit16ToValue(const char32_t c) noexcept {
    return c <= '9' ? c - '0' : (c | 0x20) - 'a' + 10;
}

constexpr std::uint8_t Digit10ToValue(const char32_t c) noexcept {
    return c - '0';
}

constexpr std::uint8_t DigitToValue(const char32_t c, std::size_t radix) noexcept {
    if (radix == 16)
        return Digit16ToValue(c);
    return Digit10ToValue(c);
}

}
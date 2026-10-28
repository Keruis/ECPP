#pragma once

#include "../algorithms.h"
#include "utility/unicode/unicode_char.h"

namespace UTILITY {

constexpr bool IsUnicodeWhitespace(char32_t cp) noexcept {
    return UTILITY::IsAnyOf(cp, U' ', U'\t', U'\v', U'\f');
}

constexpr bool IsUnicodeLineTerminator(char32_t cp) noexcept {
    return UTILITY::IsAnyOf(cp, U'\n', U'\r');
}

constexpr bool IsXidStart(char32_t cp) noexcept {
    ADT::UnicodeCharRange ranges(xid_start_range);
    return ranges.Contains(cp);
}

constexpr bool IsXidContinue(char32_t cp) noexcept {
    ADT::UnicodeCharRange ranges(xid_continue_range);
    return ranges.Contains(cp);
}

}
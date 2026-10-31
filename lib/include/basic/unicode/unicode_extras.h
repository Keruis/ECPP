#pragma once

#include <ADT/unicode_char_range.h>
#include <utility/algorithms.h>
#include <basic/char_extras.h>
#include <basic/unicode/unicode_char.h>

namespace BASIC {

constexpr bool IsXidStart(char32_t cp) noexcept {
    ADT::UnicodeCharRange ranges(xid_start_range);
    return ranges.Contains(cp);
}

constexpr bool IsXidContinue(char32_t cp) noexcept {
    ADT::UnicodeCharRange ranges(xid_continue_range);
    return ranges.Contains(cp);
}

constexpr bool IsUnicodeNameChar(char32_t cp) noexcept {
    return IsUppercase(cp) || IsDigit(cp, 10) || cp == U'-' || cp == ' ';
} 

}
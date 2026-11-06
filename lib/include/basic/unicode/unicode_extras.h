#pragma once

#include <ADT/unicode_char_range.h>
#include <utility/algorithms.h>
#include <basic/char_extras.h>
#include <basic/unicode/unicode_char.h>

namespace BASIC {

constexpr bool IsXidStart(const char32_t cp) noexcept {
    ADT::UnicodeCharRange ranges(xid_start_range);
    return ranges.Contains(cp);
}

constexpr bool IsXidContinue(const char32_t cp) noexcept {
    ADT::UnicodeCharRange ranges(xid_continue_range);
    return ranges.Contains(cp);
}

constexpr bool IsUnicodeNameChar(const char32_t cp) noexcept {
    return IsAlphanumeric(cp) || cp == U'-' || cp == ' ';
} 

}
#pragma once

#include <ADT/unicode_char_range.h>
#include <utility/algorithms.h>
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

}
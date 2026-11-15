#pragma once

#include <basic/unicode/UnicodeSet.h>

namespace BASIC::UNICODE {

constexpr bool IsXidStart(const char32_t cp) noexcept {
    return CodePointRangeSet{XidStartRange}.Contains(cp);
}

constexpr bool IsXidContinue(const char32_t cp) noexcept {
    return CodePointRangeSet{XidContinueRange}.Contains(cp);
}

}
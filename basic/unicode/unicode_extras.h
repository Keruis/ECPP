#pragma once

#include "algorithms.h"

namespace ECPP::BASIC {

constexpr bool IsUnicodeWhitespace(char32_t cp) noexcept {
    return UTILITY::IsAnyOf(cp, U' ', U'\t', U'\v', U'\f');
}

constexpr bool IsUnicodeLineTerminator(char32_t cp) noexcept {
    return UTILITY::IsAnyOf(cp, U'\n', U'\r');
}

}
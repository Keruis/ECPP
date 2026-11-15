#pragma once

#include <basic/ascii/CharClass.h>
#include <basic/unicode/UnicodeClass.h>

namespace LANG::BASIC {

constexpr bool IsNondigit(const char32_t cp) noexcept {
    return ::BASIC::ASCII::IsLetter(cp) || cp == U'_';
}

constexpr bool IsIdentifierStart(const char32_t cp) noexcept {
    return cp == U'_' || ::BASIC::UNICODE::IsXidStart(cp);
}

constexpr bool IsIdentifierContinue(const char32_t cp) noexcept {
    return ::BASIC::UNICODE::IsXidStart(cp) || ::BASIC::UNICODE::IsXidContinue(cp);
}

constexpr bool IsUnicodeScalarValue(const char32_t cp) noexcept {
    return cp <= 0x10FFFF && !(cp >= 0xD800 && cp <= 0xDFFF);
}

constexpr bool IsControlCharacter(const char32_t cp) noexcept {
    return (cp < 0x20) || (cp >= 0x7F && cp <= 0x9F);
}

constexpr bool IsBasicCharacter(const char32_t cp) noexcept {
    return cp >= 0x20 && cp <= 0x7E;
}

constexpr bool IsUnicodeNameCharacter(const char32_t cp) noexcept {
    return ::BASIC::ASCII::IsUppercase(cp) || ::BASIC::ASCII::IsDigit10(cp) || UTILITY::IsAnyOf(cp, U'_', U'-');
}

}
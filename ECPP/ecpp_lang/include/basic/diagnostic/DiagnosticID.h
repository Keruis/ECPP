#pragma once

#include <basic/diagnostic/DiagnosticLevel.h>

#include <array>
#include <string_view>

namespace LANG::BASIC::DIAGNOSTIC {

enum class DiagnosticID : std::uint8_t {
    UNTERMINATED_BLOCK_COMMENT = 0,
    INVALID_CHARACTER,
    INVALID_UNIVERSAL_CHARACTER_NAME,
    INVALID_IDENTIFIER_CHARACTER,
    UCN_DESIGNATES_CONTROL_CHARACTER,
    UCN_DESIGNATES_BASIC_CHARACTER,
    UNTERMINATED_STRING,
    UNTERMINATED_CHARACTER_LITERAL,
    EMPTY_CHARACTER_LITERAL
};

struct DiagnosticDefinition {
    DiagnosticLevel level;
    std::string_view format;
};

inline constexpr std::array<DiagnosticDefinition, 9> DiagnosticTable {{
    {DiagnosticLevel::ERROR, "unterminated block comment"},
    {DiagnosticLevel::ERROR, "invalid character '{}'"},
    {DiagnosticLevel::ERROR, "invalid universal character name"},
    {DiagnosticLevel::ERROR, "unicode character U+{:04X} is not valid in an identifier"},
    {DiagnosticLevel::ERROR, "universal character name cannot designate a control character"},
    {DiagnosticLevel::ERROR, "universal character name cannot designate a character from the basic character set"},
    {DiagnosticLevel::ERROR, "unterminated string"},
    {DiagnosticLevel::ERROR, "unterminated character literal"},
    {DiagnosticLevel::ERROR, "character literal must contain at least one character"}
}};

}
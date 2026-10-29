#pragma once

#include <cstdint>

namespace ECPP::LEXER {

enum class TokenClass : std::uint8_t {
    IDENTIFIER,
    KEYWORD,
    OPERATOR,
    LITERAL,
    PUNCTUATOR,
    UNEXPECTED_END,
    END
};

}
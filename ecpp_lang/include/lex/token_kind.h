#pragma once

#include <cstdint>

namespace ECPP::PARSER {

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
#pragma once

#include <lex/token.h>

#include <utility>

namespace ECPP::AST {

constexpr std::uint8_t post_operator_offset = 1;

enum class UnaryOperator {
    PLUS = std::to_underlying(LEXER::TokenType::OPERATORS_ADD),
    MINUS = std::to_underlying(LEXER::TokenType::OPERATORS_SUB),
    LOGICAL_NOT = std::to_underlying(LEXER::TokenType::OPERATORS_NOT),
    PRE_INC = std::to_underlying(LEXER::TokenType::OPERATORS_INC),
    POST_INC = PRE_INC + post_operator_offset,
    PRE_DEC = std::to_underlying(LEXER::TokenType::OPERATORS_DEC),
    POST_DEC = PRE_DEC + post_operator_offset
};

enum class BinaryOperator {
    ADD
};

}
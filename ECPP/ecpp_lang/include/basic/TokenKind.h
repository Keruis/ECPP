#pragma once

#include <cstdint>

namespace LANG::BASIC {

enum class TokenKind : std::uint8_t {
    END,
    IDENTIFIER,
    KEYWORD,
    LITERAL,
    OPERATOR,
    PUNCTUATOR
};

constexpr std::uint8_t RepeatedCharOperatorOffset = 90;
constexpr std::uint8_t EqualsSuffixOperatorOffset = 95;

enum class TokenType : std::uint8_t {
    IDENTIFIER = 0,
    IDENTIFIER_UCN = 1,

    OPERATORS_NOT = '!',
    OPERATORS_MOD = '%',

    PUNCTUATORS_LPAREN = '(',
    PUNCTUATORS_RPAREN = ')',

    OPERATORS_MUL = '*',
    OPERATORS_ADD = '+', 

    PUNCTUATORS_COMMA = ',',

    OPERATORS_SUB = '-',
    OPERATORS_DIV = '/',

    PUNCTUATORS_COLON = ':',
    PUNCTUATORS_SEMICOLON = ';',

    OPERATORS_LT = '<',
    OPERATORS_ASSIGN = '=',
    OPERATORS_GT = '>',

    PUNCTUATORS_LBRACKET = '[',
    PUNCTUATORS_RBRACKET = ']',
    PUNCTUATORS_LBRACE = '{',
    PUNCTUATORS_RBRACE = '}',

    OPERATORS_NEQ = '!' + EqualsSuffixOperatorOffset,

    OPERATORS_INC = '+' + RepeatedCharOperatorOffset,
    OPERATORS_DEC = '-' + RepeatedCharOperatorOffset,

    OPERATORS_PE = '+' + EqualsSuffixOperatorOffset,
    OPERATORS_RE = '%' + EqualsSuffixOperatorOffset,
    OPERATORS_ME = '*' + EqualsSuffixOperatorOffset,
    OPERATORS_SE = '-' + EqualsSuffixOperatorOffset,
    OPERATORS_DE = '/' + EqualsSuffixOperatorOffset,

    OPERATORS_LSHIFT = '<' + RepeatedCharOperatorOffset,
    OPERATORS_RSHIFT = '>' + RepeatedCharOperatorOffset,

    OPERATORS_LE = '<' + EqualsSuffixOperatorOffset,
    OPERATORS_EQ = '=' + EqualsSuffixOperatorOffset,
    OPERATORS_GE = '>' + EqualsSuffixOperatorOffset,

    KEYWORD_IF = 161,
    KEYWORD_ELSE = 162,
    KEYWORD_TRUE = 163,
    KEYWORD_FALSE = 164,

    LITERALS_NUMERIC = 225,
    LITERALS_STRING = 226,

    LITERALS_CHARACTER = 228,

    END = 255
};

}
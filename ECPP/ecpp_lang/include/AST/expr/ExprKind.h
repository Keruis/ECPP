#pragma once

#include <cstdint>

namespace LANG::AST {

enum class ExprKind : std::uint8_t {
    LITERAL_EXPR,
    NAME_EXPR,
    UNARY_EXPR,
    BINARY_EXPR
};

}
#pragma once

namespace ECPP::AST {

enum class ExprKind {
    LITERAL_EXPR,
    NAME_EXPR,
    UNARY_EXPR,
    BINARY_EXPR
};

}
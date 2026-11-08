#pragma once

#include <AST/node.h>
#include <AST/expr/expr_kind.h>

namespace ECPP::AST {

class Expr : public Node {
    public:
        using Node::Node;

        virtual ExprKind Kind() const noexcept = 0;
};

using ExprRef = Expr*;

}
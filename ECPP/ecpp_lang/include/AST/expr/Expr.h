#pragma once

#include <AST/Node.h>
#include <AST/expr/ExprKind.h>

namespace LANG::AST {

class Expr : public Node {
    public:
        using Node::Node;

        virtual ExprKind Kind() const noexcept = 0;
};

using ExprRef = Expr*;

}
#pragma once

#include <AST/node.h>
#include <AST/stmt/stmt_kind.h>

namespace ECPP::AST {

class Stmt : public Node {
    public:
        using Node::Node;

        virtual StmtKind Kind() const noexcept = 0;
};

}
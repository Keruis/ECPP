#pragma once

#include <AST/node.h>
#include <AST/decl/decl_kind.h>

namespace ECPP::AST {

class Decl : public Node {
    public:
        using Node::Node;

        virtual DeclKind Kind() const noexcept = 0;
};

using DeclRef = Decl*;

}
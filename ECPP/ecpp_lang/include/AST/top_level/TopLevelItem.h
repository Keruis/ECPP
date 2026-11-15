#pragma once

#include <AST/Node.h>

namespace LANG::AST {

class TopLevelItem : public Node {
    public:
        using Node::Node;

        virtual ~TopLevelItem() = default;
};

using TopLevelItemRef = TopLevelItem*;

}
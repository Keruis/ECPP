#pragma once

#include <AST/top_level/TopLevelItem.h>
#include <AST/top_level/stmt/StmtKind.h>

namespace LANG::AST {

class Stmt : public TopLevelItem {
    public:
        using TopLevelItem::TopLevelItem;

        virtual StmtKind Kind() const noexcept = 0;
};

using StmtRef = Stmt*;

}
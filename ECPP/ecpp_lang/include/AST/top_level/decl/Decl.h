#pragma once

#include <AST/top_level/TopLevelItem.h>
#include <AST/top_level/decl/DeclKind.h>

namespace LANG::AST {

class Decl : public TopLevelItem {
    public:
        using TopLevelItem::TopLevelItem;

        virtual DeclKind Kind() const noexcept = 0;
};

using DeclRef = Decl*;

}
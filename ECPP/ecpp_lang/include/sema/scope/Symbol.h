#pragma once

#include <sema/scope/SymbolKind.h>
#include <AST/top_level/decl/Decl.h>

namespace LANG::SEMA {

struct Symbol {
    SymbolKind kind;
    AST::DeclRef decl;
};

}
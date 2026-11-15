#pragma once

#include <sema/scope/Scope.h>

namespace LANG::SEMA {

SymbolTable& Scope::Table() noexcept {
    return m_table;
}

ScopeKind Scope::Kind() const noexcept {
    return m_kind;
}

std::size_t Scope::Depth() const noexcept {
    return m_depth;
}

}
#include <sema/scope/ScopeContext.h>

#include <ranges>

namespace LANG::SEMA {

ScopeContext::ScopeContext() {
    m_scopes.emplace_back(ScopeKind::GLOBAL, 0);
}

void ScopeContext::CreateScope(ScopeKind kind) {
    m_scopes.emplace_back(kind, m_scopes.size());
}

void ScopeContext::DeleteScope() {
    m_scopes.pop_back();
}

const Symbol* ScopeContext::LookupSymbol(std::u8string_view name) {
    for (auto& scope : m_scopes | std::views::reverse) {
        if (auto symbol = scope.Table().FindSymbol(name))
            return symbol;
    }

    return nullptr;    
}

const Symbol* ScopeContext::LookupLocalSymbol(std::u8string_view name) {
    return m_scopes.back().Table().FindSymbol(name);
}

void ScopeContext::DeclareSymbol(std::u8string_view name, const Symbol& symbol) {
    m_scopes.back().Table().CreateSymbol(name, symbol);
}

}
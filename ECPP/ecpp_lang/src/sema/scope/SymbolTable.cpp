#include <sema/scope/SymbolTable.h>

namespace LANG::SEMA {

const Symbol* SymbolTable::FindSymbol(std::u8string_view name) {
    auto it = m_symbols.find(name);

    if (it == m_symbols.end())
        return nullptr;

    return &it->second;
}

void SymbolTable::CreateSymbol(std::u8string_view name, const Symbol & symbol) {
    m_symbols.emplace(name, symbol);
}

}
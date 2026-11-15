#pragma once

#include <AST/top_level/TopLevelItem.h>

#include <vector>

namespace LANG::AST {

class TranslationUnit {
    public:
        using ItemList = std::vector<TopLevelItemRef>;

        ItemList& Items() noexcept {
            return m_items;
        }

    private:
        ItemList m_items;
};

}
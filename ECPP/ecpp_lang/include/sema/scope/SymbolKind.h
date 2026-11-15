#pragma once

#include <cstdint>

namespace LANG::SEMA {

enum SymbolKind : std::uint8_t {
    VALUE,
    TYPE
};

}
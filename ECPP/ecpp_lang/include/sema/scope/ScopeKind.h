#pragma once

#include <cstdint>

namespace LANG::SEMA {

enum class ScopeKind : std::uint8_t {
    GLOBAL,
    BLOCK
};

}
#pragma once

#include <cstdint>

namespace LANG::AST {

enum class StmtKind : std::uint8_t {
    EXPR_STMT,
    DECL_STMT,
    BLOCK_STMT
};

}
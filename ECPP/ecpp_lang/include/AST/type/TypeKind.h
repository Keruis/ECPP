#pragma once

#include <cstdint>

namespace LANG::AST {

enum TypeKind : std::uint8_t {
    BUILTIN,
    POINTER
};

enum class BuiltinTypeKind : std::uint8_t {
    VOID,
    BOOL,
    INT,
    LONG,
};

}
#pragma once

#include <AST/type/TypeKind.h>

namespace LANG::AST {

class Type {
public:
    explicit Type(TypeKind kind) noexcept
        : m_kind(kind)
    {}

    virtual ~Type() = default;

    TypeKind Kind() const noexcept {
        return m_kind;
    }

private:
    TypeKind m_kind;
};

using TypeRef = const Type*;

}
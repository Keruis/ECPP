#pragma once

#include <AST/type/Type.h>

namespace LANG::AST {

class BuiltinType final : public Type {
    public:
        explicit BuiltinType(BuiltinTypeKind kind)
            : Type(TypeKind::BUILTIN), m_builtin_kind(kind)
        {}

        BuiltinTypeKind BuiltinKind() const noexcept {
            return m_builtin_kind;
        }

    private:
        BuiltinTypeKind m_builtin_kind;
};

using BuiltinTypeRef = BuiltinType*;

}
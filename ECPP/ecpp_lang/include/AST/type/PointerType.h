#pragma once

#include <AST/type/Type.h>
#include <AST/type/QualType.h>

namespace LANG::AST {

class PointerType final : public Type {
    public:
        explicit PointerType(QualType type)
            : Type(TypeKind::POINTER), m_pointer_type(type)
        {}

        QualType Pointer() const noexcept {
            return m_pointer_type;
        }

    private:
        QualType m_pointer_type;
};

using PointerTypeRef = PointerType*;

}
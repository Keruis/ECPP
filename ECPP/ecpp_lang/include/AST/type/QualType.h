#pragma once

#include <AST/type/Type.h>

#include <functional>

namespace LANG::AST {

enum class TypeQualifier : std::uint8_t {
    NONE,
    CONST,
};

class QualType {
    public:
        explicit QualType(TypeRef type, TypeQualifier qualifiers = TypeQualifier::NONE)
            : m_type(type), m_qualifiers(qualifiers)
        {}

        TypeRef Type() const noexcept {
            return m_type;
        }

        TypeQualifier Qualifier() const noexcept {
            return m_qualifiers;
        }

    private:
        TypeQualifier m_qualifiers;
        TypeRef m_type;
};

struct QualTypeHash {
    std::size_t operator()(QualType type) const noexcept {
        const std::size_t type_hash = std::hash<TypeRef>{}(type.Type());
        const std::size_t qualifier_hash = std::hash<TypeQualifier>{}(type.Qualifier());

        return type_hash ^ (qualifier_hash << 1);;
    }
};

}
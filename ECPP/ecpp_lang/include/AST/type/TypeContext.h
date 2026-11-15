#pragma once

#include <AST/ASTResource.h>
#include <AST/type/BuiltinType.h>
#include <AST/type/PointerType.h>

#include <array>
#include <unordered_map>

namespace LANG::AST {

class TypeContext {
    public:
        TypeContext(ASTResource& resource) 
            : m_resource(resource)
        {
            InitBuiltinType();
        }

        QualType GetBuiltinType(BuiltinTypeKind) const noexcept;
        QualType GetPointerType(QualType);

    private:
        template <typename T, typename Arg>
        T* CreateType(Arg arg);

        void InitBuiltinType();

    private:
        ASTResource& m_resource;

        std::array<BuiltinTypeRef, 1> m_builtin_types;
        std::unordered_map<QualType, PointerTypeRef, QualTypeHash> m_pointer_types;
};

}
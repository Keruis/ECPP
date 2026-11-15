#include <AST/type/TypeContext.h>

#include <utility>

namespace LANG::AST {

template <typename T, typename Arg>
T* TypeContext::CreateType(Arg arg) {
    return m_resource.Create<T>(arg);
}

void TypeContext::InitBuiltinType() {
    m_builtin_types[0] = CreateType<BuiltinType>(BuiltinTypeKind::INT);
}

QualType TypeContext::GetBuiltinType(BuiltinTypeKind kind) const noexcept {
    return QualType{m_builtin_types[std::to_underlying(kind)]};
}

QualType TypeContext::GetPointerType(QualType pointer) {
    auto it = m_pointer_types.find(pointer);
    if (it != m_pointer_types.end())
        return QualType{it->second};

    const auto type = CreateType<PointerType>(pointer);

    m_pointer_types.emplace(pointer, type);

    return QualType{type};
}

}
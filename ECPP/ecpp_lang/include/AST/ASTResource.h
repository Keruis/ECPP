#pragma once

#include <AST/Node.h>

#include <memory_resource>

namespace LANG::AST {

class ASTResource {
    public:
        ASTResource() = default;

        ASTResource(const ASTResource&) = delete;
        ASTResource(ASTResource&&) = delete;
        ASTResource& operator=(const ASTResource&) = delete;
        ASTResource& operator=(ASTResource&&) = delete;

        template <typename T, typename... Args>
        T* Create(Args&&... args);

        std::pmr::memory_resource* Resource() noexcept;

    private:
        std::pmr::monotonic_buffer_resource m_resource;
};

template <typename T, typename... Args>
T* ASTResource::Create(Args&&... args) {
    std::pmr::polymorphic_allocator<T> allocator{
        &m_resource
    };

    T* node = allocator.allocate(1);

    try {
        std::construct_at(
            node,
            std::forward<Args>(args)...
        );
    } catch (...) {
        allocator.deallocate(node, 1);
        throw;
    }

    return node;
}

}
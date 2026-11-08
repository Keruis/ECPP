#pragma once

#include <AST/node.h>

#include <concepts>
#include <memory_resource>

namespace ECPP::AST {

class ASTResource {
    public:
        ASTResource() = default;

        ASTResource(const ASTResource&) = delete;
        ASTResource(ASTResource&&) = delete;
        ASTResource& operator=(const ASTResource&) = delete;
        ASTResource& operator=(ASTResource&&) = delete;

        template <typename T, typename... Args>
            requires std::derived_from<T, Node>
        T* Create(Args&&... args) {
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

        std::pmr::memory_resource* Resource() noexcept {
            return &m_resource;
        }

    private:
        std::pmr::monotonic_buffer_resource m_resource;
};

}
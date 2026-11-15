#include <AST/ASTResource.h>

namespace LANG::AST {

std::pmr::memory_resource* ASTResource::Resource() noexcept {
    return &m_resource;
}

}
#pragma once

#include <basic/source/SourceRange.h>
#include <basic/source/SourceLocation.h>

namespace LANG::AST {

class Node {
    public:
        explicit Node(const ::BASIC::SOURCE::SourceLocation loc, const ::BASIC::SOURCE::SourceRange range) noexcept
            : m_location(loc), m_range(range)
        {}

        virtual ~Node() = default;

        const ::BASIC::SOURCE::SourceLocation& Location() const noexcept {
            return m_location;
        }

        const ::BASIC::SOURCE::SourceRange& Range() const noexcept {
            return m_range;
        }

    private:
        ::BASIC::SOURCE::SourceLocation m_location;
        ::BASIC::SOURCE::SourceRange m_range;
};

}
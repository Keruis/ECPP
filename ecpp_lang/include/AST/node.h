#pragma once

#include <basic/location.h>
#include <basic/source/source_range.h>

namespace ECPP::AST {

class Node {
    public:
        explicit Node(const BASIC::Location loc, const BASIC::SourceRange range) noexcept
            : m_location(loc), m_range(range)
        {}

        virtual ~Node() = default;

        const BASIC::Location& Location() const noexcept {
            return m_location;
        }

        const BASIC::SourceRange& Range() const noexcept {
            return m_range;
        }

    private:
        BASIC::Location m_location;
        BASIC::SourceRange m_range;
};

}
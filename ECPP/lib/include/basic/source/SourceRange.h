#pragma once

#include <basic/source/SourceLocation.h>

namespace BASIC::SOURCE {

struct SourceRange {
    SourceLocation begin{};
    SourceLocation end{};

    constexpr std::size_t ByteLength() const noexcept {
        return end.offset - begin.offset;
    }
};

}
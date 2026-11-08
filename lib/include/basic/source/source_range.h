#pragma once

#include <cstddef>

namespace BASIC {

struct SourceRange {
    std::size_t begin = 0;
    std::size_t end = 0;

    constexpr std::size_t Size() const noexcept {
        return end - begin;
    }

    constexpr bool Empty() const noexcept {
        return begin == end;
    }
};

}
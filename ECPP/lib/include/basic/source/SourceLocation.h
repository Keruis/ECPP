#pragma once

#include <cstddef>

namespace BASIC::SOURCE {

struct SourceLocation {
    std::size_t line = 1;
    std::size_t column = 1;
    std::size_t offset = 0;
};

}
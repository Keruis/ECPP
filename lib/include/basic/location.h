#pragma once

#include <cstddef>

namespace BASIC {

struct Location {
    std::size_t line = 1;
    std::size_t column = 1;
    std::size_t length = 0;
};

}
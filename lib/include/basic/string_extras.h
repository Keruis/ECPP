#pragma once

#include <basic/char_extras.h>

namespace BASIC {

template <std::size_t r>
std::size_t ScanNumeric(const char8_t* start, const char8_t* end) noexcept {
    auto cur = start;
    while (cur < end && IsDigit<r>(*cur))
        cur++;
    return cur - start;
}

}
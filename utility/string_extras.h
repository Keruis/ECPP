#pragma once

#include "utility/char_extras.h"

namespace UTILITY {

template <std::size_t r>
std::size_t ScanNumeric(const char* start, const char* end) noexcept {
    auto cur = start;
    while (cur < end && UTILITY::IsDigit<r>(*cur))
        cur++;
    return cur - start;
}

}
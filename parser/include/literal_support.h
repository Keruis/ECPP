#pragma once

#include "string_extras.h"

namespace ECPP::PARSER {

template <std::size_t r>
std::size_t ScanNumeric(const char* start, const char* end) noexcept {
    auto cur = start;
    while (cur < end && SUPPORT::IsDigit<r>(*cur))
        cur++;
    return cur - start;
}

}
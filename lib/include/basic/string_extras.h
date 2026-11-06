#pragma once

#include <basic/char_extras.h>
#include <utility/integer_extras.h>

namespace BASIC {

template <auto F, typename... TS>
std::size_t ScanWhile(const char8_t* start, const char8_t* end, TS&&... args) {
    auto cur = start;
    while (cur < end && F(*cur, args...))
        cur++;
    return cur - start;
}

std::size_t ScanNumeric(const char8_t*, const char8_t*, const std::size_t) noexcept;

std::size_t IntegerStringToNumeric(const char8_t*, const char8_t*, bool&, const std::size_t, const std::size_t) noexcept;

}
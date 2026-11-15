#pragma once

#include <cstddef>

namespace BASIC::STRING {

template <auto F, typename... TS>
std::size_t ScanWhile(const char8_t* const start, const char8_t* end, TS... args) {
    auto cur = start;
    while (cur < end && F(*cur, args...))
        cur++;
    return cur - start;
}

std::size_t ScanNumeric(const char8_t* const, const char8_t* const, const std::size_t) noexcept;
std::size_t IntegerStringToNumeric(const char8_t*, const char8_t*, bool&, const std::size_t, const std::size_t) noexcept;

}
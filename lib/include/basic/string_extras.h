#pragma once

#include <basic/char_extras.h>
#include <utility/integer_extras.h>

#include <concepts>
#include <utility>

namespace BASIC {

template <auto F, typename... TS>
std::size_t ScanWhile(const char8_t* start, const char8_t* end, TS&&... args) 
    noexcept(noexcept((static_cast<bool>(F(std::declval<const char>(), std::declval<TS&>()...)))))
requires (requires (const char8_t c) {
    {F(c, std::declval<TS&>()...)} -> std::convertible_to<bool>;
}) {
    auto cur = start;
    while (cur < end && F(*cur, args...))
        cur++;
    return cur - start;
}

std::size_t ScanNumeric(const char8_t*, const char8_t*, std::size_t) noexcept;

std::size_t IntegerStringToNumeric(const char8_t*, const char8_t*, bool&, std::size_t, std::size_t) noexcept;

}
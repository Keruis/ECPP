#pragma once

#include <functional>
#include <type_traits>

namespace UTILITY {

template <auto pred = std::equal_to<>{}, typename... Ts>
constexpr bool IsAnyOf(const auto& value, const Ts&&... candidates) noexcept((noexcept(pred(value, candidates)) && ...)) {
    return (pred(value, candidates) || ...);
}

template <auto pred = std::equal_to<>{}, typename... Ts>
constexpr bool IsAllOf(const auto& value, const Ts&&... candidates) noexcept((noexcept(pred(value, candidates)) && ...)) {
    return (pred(value, candidates) && ...);
}

template <auto& FT, auto First, decltype(First)... Rest, typename T = decltype(First), typename UT = std::underlying_type_t<T>>
    requires (std::is_enum_v<T>)
bool HasAnyFlag(auto f) noexcept {
    constexpr UT mask = (static_cast<UT>(First) | ... | static_cast<UT>(Rest));
    return (FT[f] & mask);
}

template <std::size_t left, std::size_t right>
struct Interval {
    constexpr static bool IsInClosedInterval(std::size_t value) noexcept {
        return value >= left && value <= right; 
    }

    constexpr static bool IsInOpeninterval(std::size_t value) noexcept {
        return value > left && value < right;
    }

    constexpr static bool IsInLeftClosedInterval(std::size_t value) noexcept {
        return value >= left && value < right; 
    }

    constexpr static bool IsInLeftOpeninterval(std::size_t value) noexcept {
        return value > left && value <= right;
    }
};

}
#pragma once

#include <functional>

namespace ECPP::UTILITY {

template <auto pred = std::equal_to<>{}, typename... Ts>
constexpr bool IsAnyOf(const auto& value, const Ts&&... candidates) noexcept((noexcept(pred(value, candidates)) && ...)) {
    return (pred(value, candidates) || ...);
}

template <auto pred = std::equal_to<>{}, typename... Ts>
constexpr bool IsAllOf(const auto& value, const Ts&&... candidates) noexcept((noexcept(pred(value, candidates)) && ...)) {
    return (pred(value, candidates) && ...);
}

}
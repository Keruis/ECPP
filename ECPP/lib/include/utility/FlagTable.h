#pragma once

#include <type_traits>

namespace UTILITY {

template <auto& FT, auto First, decltype(First)... Rest, typename T = decltype(First), typename UT = std::underlying_type_t<T>>
    requires (std::is_enum_v<T>)
bool HasAnyFlag(auto f) noexcept {
    constexpr UT mask = (static_cast<UT>(First) | ... | static_cast<UT>(Rest));
    return (FT[f] & mask);
}

}
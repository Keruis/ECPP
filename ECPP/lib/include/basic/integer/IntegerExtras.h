#pragma once

namespace BASIC::INTEGER {

constexpr bool WouldOverflow(const auto value, const auto digit, const auto M, const auto radix) noexcept {
    return value > (M - digit) / radix;
}

}
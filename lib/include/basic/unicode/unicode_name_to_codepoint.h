#pragma once

#include <cstddef>
#include <cstdint>
#include <string>
#include <string_view>

namespace BASIC {

constexpr std::u8string LooseUnicodeName(std::u8string_view);

constexpr std::int32_t HangulJamoIndex(std::u8string_view&, const std::size_t, const std::size_t);
constexpr bool HangulNameToCodepoint(std::u8string_view, char32_t&);

constexpr bool GeneratedNameToCodepoint(std::u8string_view, char32_t&);

constexpr bool UnicodeNameToCodepoint(std::u8string_view, char32_t&);

}
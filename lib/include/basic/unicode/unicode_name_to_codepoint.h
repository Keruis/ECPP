#pragma once

#include <cstddef>
#include <cstdint>
#include <string>
#include <string_view>

namespace BASIC {

struct UnicodeNameNode {
    bool has_code_point = false;
    bool has_sibling = false;
    std::u8string_view name;
    std::uint32_t code_point = 0;
    std::uint32_t children_offset = 0;
    std::size_t size = 0;
};

std::u8string LooseUnicodeName(std::u8string_view);

std::int32_t HangulJamoIndex(std::u8string_view&, const std::size_t, const std::size_t);
bool HangulNameToCodepoint(std::u8string_view, char32_t&);

bool GeneratedNameToCodepoint(std::u8string_view, char32_t&);

UnicodeNameNode ReadNode(std::uint32_t);
bool NameToCodepoint(std::u8string_view, char32_t&);

bool UnicodeNameToCodepoint(std::u8string_view, char32_t&);

}
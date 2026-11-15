#pragma once

#include <cstdint>
#include <string_view>

namespace BASIC::UNICODE {

struct UnicodeNameNode {
    bool has_code_point = false;
    bool has_sibling = false;
    std::u8string_view name;
    std::uint32_t code_point = 0;
    std::uint32_t children_offset = 0;
    std::size_t size = 0;
};

class UnicodeNameToCodepoint {
    public:
        static bool Lookup(const std::u8string_view, char32_t&);

    private:
        static std::int32_t HangulJamoIndex(std::u8string_view&, const std::size_t, const std::size_t);
        static bool HangulNameToCodepoint(const std::u8string_view, char32_t&);
        static bool GeneratedNameToCodepoint(const std::u8string_view, char32_t&);
        static bool NameToCodepoint(std::u8string_view, char32_t&);

        static UnicodeNameNode ReadNode(const std::uint32_t);
};

    
}
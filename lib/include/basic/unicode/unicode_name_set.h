#pragma once

#include <vector>
#include <cstdint>
#include <string_view>
#include <ADT/unicode_char_range.h>

namespace BASIC {

constexpr std::array<std::array<std::u8string_view, 28>, 3> hangul_lables {{
    {u8"g", u8"gg", u8"n", u8"d", u8"dd", u8"r", u8"m", u8"b", u8"bb", u8"s", u8"ss", u8"", u8"j", u8"jj", u8"c", u8"k", u8"t", u8"p", u8"h"},
    {u8"a", u8"ae", u8"ya", u8"yae", u8"eo", u8"e", u8"yeo", u8"ye", u8"o", u8"wa", u8"wae", u8"oe", u8"yo", u8"u", u8"weo", u8"we", u8"wi", u8"yu", u8"eu", u8"yi", u8"i"},
    {u8"", u8"g", u8"gg", u8"gs", u8"n", u8"nj", u8"nh", u8"d", u8"l", u8"lg", u8"lm", u8"lb", u8"ls", u8"lt", u8"lp", u8"lh", u8"m", u8"b", u8"bs", u8"s", u8"ss", u8"ng", u8"j", u8"c", u8"k", u8"t", u8"p", u8"h"}
}};

struct GeneratedNameData {
    std::u8string_view prefix;
    ADT::UnicodeCharRange::CharRanges range;
};

constexpr std::array<GeneratedNameData, 20> generated_names_prefix_labels {{
    {u8"cjkunifiedideograph", {0x3400, 0x4DBF}},
    {u8"cjkunifiedideograph", {0x4E00, 0x9FFF}},
    {u8"cjkunifiedideograph", {0x20000, 0x2A6DF}},
    {u8"cjkunifiedideograph", {0x2A700, 0x2B73F}},
    {u8"cjkunifiedideograph", {0x2B740, 0x2B81E}},
    {u8"cjkunifiedideograph", {0x2B820, 0x2CEAD}},
    {u8"cjkunifiedideograph", {0x2CEB0, 0x2EBE0}},
    {u8"cjkunifiedideograph", {0x2EBF0, 0x2EE5D}},
    {u8"cjkunifiedideograph", {0x30000, 0x3134A}},
    {u8"cjkunifiedideograph", {0x31350, 0x323AF}},
    {u8"cjkunifiedideograph", {0x323B0, 0x33479}},
    {u8"egyptianhieroglyph", {0x13460, 0x143FA}},
    {u8"tangutideograph", {0x17000, 0x187F7}},
    {u8"tangutideograph", {0x18D00, 0x18D08}},
    {u8"khitansmallscriptcharacter", {0x18B00, 0x18CD5}},
    {u8"nushucharacter", {0x1B170, 0x1B2FB}},
    {u8"sealcharacter", {0x3D000, 0x3FC3F}},
    {u8"cjkcompatibiilityideograph", {0xF900, 0xFA6D}},
    {u8"cjkcompatibiilityideograph", {0xFA70, 0xFAD9}},
    {u8"cjkcompatibiilityideograph", {0x2F800, 0x2FA1D}},
}};

struct NameNode {
    bool has_code_point = false;
    bool has_long_name = false;
    std::uint8_t index_or_size = 0;

    constexpr void to_bytes(std::vector<std::uint8_t>& bytes) {
        bytes.push_back((has_code_point ? 0x80 : 0) | (has_long_name ? 0x40 : 0) | index_or_size);
    }

    constexpr NameNode() = default;
    constexpr explicit NameNode(const std::uint8_t*& bytes)
        : has_code_point(*bytes & 0x80),
        has_long_name(*bytes & 0x40),
        index_or_size(*bytes & 0x3F)
    {
        bytes++;
    }
};

struct DictIndexNode {
    std::uint16_t index= 0;

    constexpr void to_bytes(std::vector<std::uint8_t>& bytes) {
        bytes.push_back(index >> 8);
        bytes.push_back(index);
    }

    constexpr DictIndexNode() = default;
    constexpr explicit DictIndexNode(const std::uint8_t*& bytes) 
        : index(bytes[0] << 8 | bytes[1])
    {
        bytes += 2;
    }
};

struct ChildrenOffsetNode {
    bool has_children = false;
    bool has_sibling = false;

    std::uint32_t children_offset = 0;

    constexpr void init(std::vector<std::uint8_t>& bytes, bool has_code_point) {
        if (has_code_point)
            bytes.push_back(0);
        else bytes.push_back((has_children ? 0x40 : 0) | (has_sibling ? 0x80 : 0));
        bytes.push_back(0);
        bytes.push_back(0);
    }

    constexpr void to_bytes(std::uint8_t* const bytes, bool has_code_point) {
        if (has_code_point)
            bytes[0] = children_offset >> 16;
        else
            bytes[0] = children_offset >> 16 | bytes[0];
        bytes[1] = children_offset >> 8;
        bytes[2] = children_offset;
    }

    constexpr ChildrenOffsetNode() = default;
    constexpr explicit ChildrenOffsetNode(const std::uint8_t*& bytes, bool has_code_point) {
        if (has_code_point) {
            children_offset = (bytes[0] << 16) | (bytes[1] << 8) | bytes[2];
            bytes += 3;
        } else {
            std::uint32_t h = bytes[0] & 0x3F;
            has_children = bytes[0] & 0x40;
            has_sibling = bytes[0] & 0x80;
            bytes++;
            if (has_children) {
                children_offset = (h << 16) | (bytes[0] << 8) | bytes[1];
                bytes += 2;
            }
        }
    }
};

struct CodePointNode {
    bool has_children = false;
    bool has_sibling = false;
    std::uint32_t code_point = 0;

    constexpr void to_bytes(std::vector<std::uint8_t>& bytes) {
        bytes.push_back((code_point >> 16) | (has_children ? 0x40 : 0) | (has_sibling ? 0x80 : 0));
        bytes.push_back(code_point >> 8);
        bytes.push_back(code_point);
    }

    constexpr CodePointNode() = default;
    constexpr explicit CodePointNode(const std::uint8_t*& bytes) 
        : has_children(bytes[0] & 0x40),
            has_sibling(bytes[0] & 0x80),
            code_point(((bytes[0] & 0x3F) << 16) | bytes[1] << 8 | bytes[2])
    {
        bytes += 3;
    }
};

// SRAER



// END

}
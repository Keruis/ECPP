#include <basic/unicode/unicode_name_to_codepoint.h>

#include <basic/unicode/unicode_name_set.h>

#include <basic/char_extras.h>
#include <basic/string_extras.h>

#include <limits>
#include <ranges>

namespace BASIC {

constexpr std::u8string LooseUnicodeName(std::u8string_view name) {
    std::u8string str;
    for (auto [i, c] : name | std::views::enumerate) {
        if (c == ' ' || c == '_') 
            continue;
        if (
            c == '-' && i != 0 && 
            i + 1 < name.size() &&
            IsAlphanumeric(static_cast<char32_t>(name[i - 1])) && 
            IsAlphanumeric(static_cast<char32_t>(name[i + 1]))
        ) continue;
        str.push_back(IsAscii(static_cast<char32_t>(c)) ? (c | 0x20) : c);
    }
    return str;
}

constexpr std::int32_t HangulJamoIndex(std::u8string_view& LVT, const std::size_t label_i, const std::size_t max) {
    const auto& labels = hangul_lables[label_i];
    std::size_t max_size = 0;
    std::int32_t max_i = -1;
    for (std::size_t i = 0; i < labels.size(); i++) {
        auto& label = labels[i];
        if (!LVT.starts_with(label))
            continue;
        if (label.size() >= max_size) {
            max_size = label.size();
            max_i = i;
        }
    }
    LVT = LVT.substr(max_size);
    return max_i;
}

constexpr bool HangulNameToCodepoint(std::u8string_view name, char32_t& cp) {
    auto LVT = name.substr(14);
    auto L = HangulJamoIndex(LVT, 0, 19);
    auto V = HangulJamoIndex(LVT, 1, 21);
    auto T = HangulJamoIndex(LVT, 2, 28);
    if (!LVT.empty() || L == -1 || V == -1 || T == -1)
        return false;
    cp = 0xAC00 + (L * 21 + V) * 28 + T;
    return true;
}

constexpr bool GeneratedNameToCodepoint(std::u8string_view name, char32_t& cp) {
    for (auto& prefix : generated_names_prefix_labels) {
        if (!name.starts_with(prefix.prefix))
            continue;
        std::u8string_view hex = name.substr(prefix.prefix.size());
        const std::size_t hex_size = ScanNumeric(hex.data(), hex.data() + hex.size(), 16);
        if (hex_size != hex.size())
            continue;
        bool is_overflow = false;
        cp = IntegerStringToNumeric(hex.data(), hex.data() + hex.size(), is_overflow, std::numeric_limits<char32_t>::max(), 16);
        if (is_overflow || cp < prefix.range.start || cp > prefix.range.end)
            continue;
        return true;
    }
    return false;
}

constexpr bool UnicodeNameToCodepoint(std::u8string_view name, char32_t& cp) {
    auto loose_name = LooseUnicodeName(name);
    if (loose_name.empty())
        return false;
    if (loose_name.starts_with(u8"hangulsyllable") && HangulNameToCodepoint(loose_name, cp))
        return true;
    if (GeneratedNameToCodepoint(loose_name, cp))
        return true;

}

}
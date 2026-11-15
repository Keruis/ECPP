#include <basic/unicode/UnicodeNameToCodepoint.h>

#include <basic/unicode/UnicodeNameSet.h>
#include <basic/string/StringAlgorithms.h>

namespace BASIC::UNICODE {

bool UnicodeNameToCodepoint::Lookup(const std::u8string_view name, char32_t& cp) {
    if (name.empty())
        return false;
    
    if (name.starts_with(u8"HANGUL SYLLABLE ") && HangulNameToCodepoint(name, cp))
        return true;
    
    if (GeneratedNameToCodepoint(name, cp))
        return true;
    
    if (NameToCodepoint(name, cp))
        return true;

    return false;
}

std::int32_t UnicodeNameToCodepoint::HangulJamoIndex(std::u8string_view& LVT, const std::size_t label_i, const std::size_t max) {
    const auto& labels = HangulLabels[label_i];
    std::size_t max_size = 0;
    std::int32_t max_i = -1;

    for (std::size_t i = 0; i < labels.size(); i++) {
        auto& label = labels[i];
        if (!LVT.starts_with(label))
            continue;
        
        if (label.size() > max_size) {
            max_size = label.size();
            max_i = i;
        }
    }
    
    LVT = LVT.substr(max_size);
    return max_i;
}

bool UnicodeNameToCodepoint::HangulNameToCodepoint(const std::u8string_view name, char32_t& cp) {
    auto LVT = name.substr(16);
    
    auto L = HangulJamoIndex(LVT, 0, 19);
    auto V = HangulJamoIndex(LVT, 1, 21);
    auto T = HangulJamoIndex(LVT, 2, 28);
    if (!LVT.empty() || L == -1 || V == -1 || T == -1)
        return false;
    
    cp = 0xAC00 + (L * 21 + V) * 28 + T;
    return true;
}

bool UnicodeNameToCodepoint::GeneratedNameToCodepoint(const std::u8string_view name, char32_t& cp) {
    for (auto& prefix : GeneratedNamePrefixLabels) {
        if (!name.starts_with(prefix.prefix))
            continue;
        
        std::u8string_view hex = name.substr(prefix.prefix.size());
        const std::size_t hex_size = STRING::ScanNumeric(hex.data(), hex.data() + hex.size(), 16);
        if (hex_size != hex.size())
            continue;
        
        bool is_overflow = false;
        cp = STRING::IntegerStringToNumeric(hex.data(), hex.data() + hex.size(), is_overflow, 0x10FFFF, 16);
        if (is_overflow || cp < prefix.range.start || cp > prefix.range.end)
            continue;

        return true;
    }
    return false;
}

bool UnicodeNameToCodepoint::NameToCodepoint(std::u8string_view name, char32_t& cp) {
    std::size_t node_offset = 0;

    while (true) {
        auto node = ReadNode(node_offset);
        if (!name.starts_with(node.name)) {
            if (!node.has_sibling)
                return false;
            
            node_offset += node.size;
            continue;
        }
        
        name = name.substr(node.name.size());
        if (name.size() == 0 && node.has_code_point) {
            cp = node.code_point;
            return true;
        }
        
        if (!node.children_offset)
            return false;
        
        node_offset = node.children_offset;
    }
}

UnicodeNameNode UnicodeNameToCodepoint::ReadNode(const std::uint32_t offset) {
    UnicodeNameNode node;
    if (offset >= UnicodeNameNodes.size())
        return node;
    
    auto cur = &UnicodeNameNodes[offset];
    NameNode name_node(cur);
    node.has_code_point = name_node.has_code_point;
    if (name_node.has_long_name) {
        DictIndexNode dict_index_node(cur);
        node.name = std::u8string_view{UnicodeNameDict.data() + dict_index_node.index, name_node.index_or_size};
    } else node.name = std::u8string_view{UnicodeNameDict.data() + name_node.index_or_size, 1};
    
    if (name_node.has_code_point) {
        CodePointNode code_point_node(cur);
        node.code_point = code_point_node.code_point;
        node.has_sibling = code_point_node.has_sibling;
        if (code_point_node.has_children) {
            ChildrenOffsetNode children_offset_node(cur, true);
            node.children_offset = children_offset_node.children_offset;
        }
    } else {
        ChildrenOffsetNode children_offset_node(cur, false);
        node.has_sibling = children_offset_node.has_sibling;
        node.children_offset = children_offset_node.children_offset;
    }

    node.size = cur - &UnicodeNameNodes[offset];
    return node;
}
    
}
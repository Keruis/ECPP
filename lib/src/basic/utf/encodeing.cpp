#include <basic/utf/encodeing.h>

#include <string>
#include <string_view>

namespace BASIC {

template <>
bool Decode<char8_t>(const std::u8string_view chars, UTFChar& c) noexcept {
    if (chars.empty()) 
        return false;
    const char8_t first = chars[0];
    switch (first) {
        case 0x00 ... 0x7F:
            c = {static_cast<char32_t>(first), 1};
            return true;
        case 0xC2 ... 0xDF: {
            if (chars.size() < 2)
                return false;
            const char8_t second = chars[1];
            if ((second & 0xC0) != 0x80)
                return false;
            const char32_t cp = ((first & 0x1F) << 6) | (second & 0x3F);
            c = {cp, 2};
            return true;
        }
        case 0xE0 ... 0xEF: {
            if (chars.size() < 3) 
                return false;
            const char8_t second = chars[1];
            const char8_t third = chars[2];
            if ((first == 0xE0 && (second < 0xA0)) || (first == 0xED && (second > 0x9F)) || UTILITY::IsAnyOf(0x80, second & 0xC0, third & 0xC0)) 
                return false;
            const char32_t cp = ((first & 0x0F) << 12) | ((second & 0x3F) << 6) | (third & 0x3F);
            c = {cp, 3};
            return true;
        }
        case 0xF0 ... 0xF4: {
            if (chars.size() < 4)
                return false;
            const char8_t second = chars[1];
            const char8_t third = chars[2];
            const char8_t fourth = chars[3];
            if ((first == 0xF0 && (second < 0x90)) || (first == 0xF0 && second < 0x90) || (first == 0xF4 && (second > 0x8F)) || UTILITY::IsAnyOf(0x80, second & 0xC0, third & 0xC0, fourth & 0xC0))
                return false;
            const char32_t cp = ((first & 0x07) << 18) | ((second & 0x3F) << 12) | ((third & 0x3F) << 6) | (fourth & 0x3F);
            c = {cp, 4};
            return true;
        }
        default:
            return false;
    }
}

template <>
bool Decode<char16_t>(const std::u16string_view chars, UTFChar &c) noexcept {
    if (chars.empty())
        return false;
    const char16_t first = chars[0];
    if (first >= 0xD800 && first <= 0xDBFF) {
        if (chars.size() < 2)
            return false;
        const char16_t second = chars[1];
        if (second < 0xDC00 || second > 0xDFFF)
            return false;
        const char32_t cp = 0x10000 + ((first - 0xD800) << 10) + (second - 0xDC00);
        c = {cp, 2};
        return true;
    }
    if (first >= 0xDC00 && first <= 0xDFFF)
        return false;
    c = {first, 1};
    return true;
}

template <>
bool Decode<char32_t>(const std::u32string_view chars, UTFChar &c) noexcept {
    if (chars.empty())
        return false;
    const char32_t cp = chars[0];
    if (cp > 0x10FFFF || (cp >= 0xD800 && cp <= 0xDFFF))
        return false;
    c = {cp, 1};
    return true;
}

template <>
bool Encode<char8_t>(char32_t cp, std::u8string& str) noexcept {
    std::size_t length = 0;
    char8_t bytes[4];
    switch (cp) {
        case 0x00 ... 0x7F:
            length = 1;
            bytes[0] = static_cast<char8_t>(cp);
            break;
        case 0x80 ... 0x7FF:
            length = 2;
            bytes[0] = (cp >> 6) | 0xC0;
            bytes[1] = (cp & 0x3F) | 0x80;
            break;
        case 0xD800 ... 0xDFFF:
            return false;
        case 0x800 ... 0xD7FF:
        case 0xE000 ... 0xFFFF:
            length = 3;
            bytes[0] = (cp >> 12) | 0xE0;
            bytes[1] = ((cp >> 6) & 0x3F) | 0x80;
            bytes[2] = (cp & 0x3F) | 0x80;
            break;
        case 0x10000 ... 0x10FFFF:
            length = 4;
            bytes[0] = (cp >> 18) | 0xF0;
            bytes[1] = ((cp >> 12) & 0x3F) | 0x80;
            bytes[2] = ((cp >> 6) & 0x3F) | 0x80;
            bytes[3] = (cp & 0x3F) | 0x80;
            break;
        default:
            return false;
    }
    str.append(bytes, length);
    return true;
}

template <>
bool Encode<char16_t>(char32_t cp, std::u16string& str) noexcept {
    switch (cp) {
        case 0x00 ... 0xD7FF:
        case 0xE000 ... 0xFFFF:
            str.push_back(static_cast<char16_t>(cp));
            return true;
        case 0x10000 ... 0x10FFFF: {
            const char32_t value = cp - 0x10000;
            str.push_back((value >> 10) + 0xD800);
            str.push_back((value & 0x3FF) + 0xDC00);
            return true;
        }
        default:
            return false;
    }
}

template <>
bool Encode<char32_t>(char32_t cp, std::u32string& str) noexcept {
    if (cp > 0x10FFFF || (cp >= 0xD800 && cp <= 0xDFFF))
        return false;
    str.push_back(cp);
    return true;
}

}
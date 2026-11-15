#include <basic/utf/Encodeing.h>

#include <utility/Predicate.h>

namespace BASIC::UTF {

bool DecodeUTF8(const std::u8string_view chars, DecodedCodePoint& c) noexcept {
    if (chars.empty())
        return false;

    const char8_t first = chars[0];
    switch (first) {
        case 0x00 ... 0x7F:
            c = {
                static_cast<char32_t>(first),
                1
            };

            return true;
        
        case 0xC2 ... 0xDF: {
            if (chars.size() < 2)
                return false;

            const char8_t second = chars[1];
            if ((second & 0xC0) != 0x80)
                return false;

            c = {
                static_cast<char32_t>(((first & 0x1F) << 6) | (second & 0x3F)),
                2
            };

            return true;
        }

        case 0xE0 ... 0xEF: {
            if (chars.size() < 3) 
                return false;

            const char8_t second = chars[1];
            const char8_t third = chars[2];
            if ((first == 0xE0 && (second < 0xA0)) || 
                (first == 0xED && (second > 0x9F)) || 
                !UTILITY::IsAllOf(0x80, second & 0xC0, third & 0xC0)
            ) return false;
            
            c = {
                static_cast<char32_t>(((first & 0x0F) << 12) | ((second & 0x3F) << 6) | (third & 0x3F)),
                3
            };

            return true;
        }

        case 0xF0 ... 0xF4: {
            if (chars.size() < 4)
                return false;

            const char8_t second = chars[1];
            const char8_t third = chars[2];
            const char8_t fourth = chars[3];
            if ((first == 0xF0 && (second < 0x90)) || 
                (first == 0xF4 && (second > 0x8F)) || 
                !UTILITY::IsAllOf(0x80, second & 0xC0, third & 0xC0, fourth & 0xC0)
            ) return false;
            
            c = {
                static_cast<char32_t>(((first & 0x07) << 18) | ((second & 0x3F) << 12) | ((third & 0x3F) << 6) | (fourth & 0x3F)),
                4            
            };

            return true;
        }

        default:
            return false; 
    }
}

bool EncodeUTF8(const char32_t cp, std::u8string& str) {
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

}
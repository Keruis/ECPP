#pragma once

#include "utf_char.h"
#include "algorithms.h"

#include <cstddef>
#include <cstdint>
#include <string_view>

namespace ECPP::BASIC {

class UTF8View {
    public:
        UTF8View(std::string_view data) 
            : m_data(data) {}

        UTFChar PeekAt(std::size_t offset) const {
            UTFChar c;
            std::size_t ByteOffset = m_offset;
            while (ByteOffset < m_data.size()) {
                const std::string_view subview = m_data.substr(ByteOffset);
                if (!Decode(subview, c)) [[unlikely]]
                    throw "ECPP::BASIC::utf8_view::Peek";
                if (!offset)
                    return c;
                ByteOffset += c.length;
                offset--;
            }
            return {0, 0};
        }

        void Advance(const std::size_t offset) {
            m_offset += offset;
        }

        bool AtEnd() const noexcept {
            return m_offset >= m_data.size();
        }

        const char* CurPtr() const noexcept {
            return m_data.data() + m_offset;
        }

        const char* End() const noexcept {
            return m_data.end();
        }

        bool Empty() const noexcept {
            return m_data.empty();
        }

    private:
        bool Decode(const std::string_view chars, UTFChar& c) const noexcept {
            if (chars.empty()) 
                return false;
            const std::uint8_t first = chars[0];
            switch (first) {
                case 0x00 ... 0x7F:
                    c = {static_cast<char32_t>(first), 1};
                    return true;
                case 0xC2 ... 0xDF: {
                    if (chars.size() < 2) [[unlikely]]
                        return false;
                    const std::uint8_t second = chars[1];
                    if ((second & 0xC0) != 0x80)
                        return false;
                    const char32_t cp = ((first & 0x1F) << 6) | (second & 0x3F);
                    c = {cp, 2};
                    return true;
                }
                case 0xE0 ... 0xEF: {
                    if (chars.size() < 3) 
                        return false;
                    const std::uint8_t second = chars[1];
                    const std::uint8_t third = chars[2];
                    if ((first == 0xE0 && (second < 0xA0)) || (first == 0xED && (second > 0x9F)) || UTILITY::IsAnyOf(0x80, second & 0xC0, third & 0xC0)) 
                        return false;
                    const char32_t cp = ((first & 0x0F) << 12) | ((second & 0x3F) << 6) | (third & 0x3F);
                    c = {cp, 3};
                    return true;
                }
                case 0xF0 ... 0xF4: {
                    if (chars.size() < 4)
                        return false;
                    const std::uint8_t second = chars[1];
                    const std::uint8_t third = chars[2];
                    const std::uint8_t fourth = chars[3];
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

    private:
        std::size_t m_offset{};
        std::string_view m_data;
};

}
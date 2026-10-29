#pragma once

#include <basic/utf/utf_char.h>
#include <basic/utf/encodeing.h>
#include <cstddef>
#include <string_view>

namespace BASIC {

template <typename T>
class UTFCursor {
    public:
        explicit UTFCursor(std::basic_string_view<T> data) 
            : m_data(data) 
        {}

        auto StartPtr() const noexcept {
            return m_data.data();
        }

        auto CurPtr() const noexcept {
            return m_data.data() + m_offset;
        }

        auto EndPtr() const noexcept {
            return m_data.data() + m_data.size();
        }

        bool AtEnd() const noexcept {
            return CurPtr() >= EndPtr();
        }

        bool Empty() const noexcept {
            return m_data.empty();
        }

        void AdvanceBytes(std::size_t offset) noexcept {
            m_offset += offset;
        }

        UTFChar PeekAt(std::size_t offset) const {
            UTFChar c;
            std::size_t CharOffset = m_offset;
            while (CharOffset < m_data.size()) {
                const std::basic_string_view<T> subview = m_data.substr(CharOffset);
                if (!Decode(subview, c)) [[unlikely]]
                    throw "BASIC::UTFCursor::PeekAt";
                if (!offset)
                    return c;
                CharOffset += c.length;
                offset--;
            }
            return {0, 0};
        }

    private:
        std::size_t m_offset{};
        std::basic_string_view<T> m_data;

};

}
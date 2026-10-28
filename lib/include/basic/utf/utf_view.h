#pragma once

#include <basic/utf/utf_char.h>
#include <basic/utf/decode.h>

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

        UTFChar Peek() const {
            if (AtEnd())
                return {0, 0};
            UTFChar c;
            if (!Decode(m_data.substr(m_offset), c))
                throw "BASIC::UTFCursor::Peek";
            return c;
        }

        UTFChar PeekNext() const {
            const UTFChar c1 = Peek();
            const std::size_t offset = m_offset + c1.length;
            if (m_data.data() + offset >= EndPtr())
                return {0, 0};
            UTFChar c2;
            if (!Decode(m_data.substr(offset), c2))
                throw "BASIC::UTFCursor::PeekNext";
            return c2;
        }

    private:
        std::size_t m_offset{};
        std::basic_string_view<T> m_data;

};

}
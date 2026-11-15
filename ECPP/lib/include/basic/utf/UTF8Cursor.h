#pragma once

#include <basic/utf/DecodedCodePoint.h>

#include <string_view>

namespace BASIC::UTF {

class UTF8Cursor {
    public:
        explicit UTF8Cursor(const std::u8string_view data)
            : m_data(data)
        {}

        const char8_t* StartPtr() const noexcept;
        const char8_t* EndPtr() const noexcept;
        const char8_t* CurPtr() const noexcept;

        bool Empty() const noexcept;
        bool AtEnd() const noexcept;
        bool HasRemaining(const std::size_t) const noexcept;

    protected:
        void AdvanceOffset(const std::size_t) noexcept;
        void RetreatOffset(const std::size_t) noexcept;

        DecodedCodePoint PeekAt(std::size_t) const;

        std::size_t Offset() const noexcept;

    private:
        std::u8string_view m_data;
        std::size_t m_offset{};
};

}
#pragma once

#include <basic/utf/UTF8Cursor.h>
#include <basic/source/SourceRange.h>
#include <type_traits>

namespace BASIC::SOURCE {

class SourceCursor final : public UTF::UTF8Cursor {
    public:
        using UTF::UTF8Cursor::UTF8Cursor;

        const SourceRange& Range() const noexcept;

        UTF::DecodedCodePoint Peek() const;
        UTF::DecodedCodePoint PeekNext() const;

        template <typename... Args>
            requires (std::is_same_v<Args, BASIC::UTF::DecodedCodePoint> && ...)
        void Advance(const Args&...) noexcept;
        void Advance(const UTF::DecodedCodePoint&) noexcept;
        void AdvanceBytes(std::size_t) noexcept;
        void Retreat(const UTF::DecodedCodePoint&) noexcept;
        void AdvanceCursor(const std::size_t) noexcept;
        void AdvanceColumn(const std::size_t) noexcept;
        
        void UpdateStartPosition() noexcept;
        void UpdateEndPosition() noexcept;

    private:
        SourceLocation CurrentLocation() const noexcept;

    private:
        SourceRange m_range{};
    
        std::size_t m_line_offset{};
        std::size_t m_column_offset{};

        bool m_last_was_cr{};
};

template <typename... Args>
    requires (std::is_same_v<Args, BASIC::UTF::DecodedCodePoint> && ...)
void SourceCursor::Advance(const Args&... args) noexcept {
    (Advance(args), ...);
}

}
#include <basic/source/SourceCursor.h>

namespace BASIC::SOURCE {

const SourceRange& SourceCursor::Range() const noexcept {
    return m_range;
}

UTF::DecodedCodePoint SourceCursor::Peek() const {
    return PeekAt(0);
}

UTF::DecodedCodePoint SourceCursor::PeekNext() const {
    return PeekAt(1);
}

void SourceCursor::Advance(const UTF::DecodedCodePoint& c) noexcept {
    AdvanceCursor(c.uint_length);
    switch (c.codepoint) {
        case U'\n':
            if (m_last_was_cr)
                break;

        case U'\r':
            m_line_offset++;
            m_column_offset = 0;
            break;

        default:
            m_column_offset++;
    }

    m_last_was_cr = c.codepoint == U'\r';
}

void SourceCursor::Retreat(const UTF::DecodedCodePoint& c) noexcept {
    RetreatOffset(c.uint_length);
    m_column_offset -= c.uint_length;
}

void SourceCursor::AdvanceBytes(std::size_t offset) noexcept {
    AdvanceCursor(offset);
    AdvanceColumn(offset);
}

void SourceCursor::AdvanceCursor(const std::size_t offset) noexcept {
    AdvanceOffset(offset);
}

void SourceCursor::AdvanceColumn(const std::size_t offset) noexcept {
    m_column_offset += offset;
    m_last_was_cr = false;
}

void SourceCursor::UpdateStartPosition() noexcept {
    m_range.begin = CurrentLocation();

    m_line_offset = 0;
    m_column_offset = 0;
}

void SourceCursor::UpdateEndPosition() noexcept {
    m_range.end = CurrentLocation();
}

SourceLocation SourceCursor::CurrentLocation() const noexcept {
    SourceLocation location = m_range.begin;

    location.line += m_line_offset;
    location.column = m_line_offset
        ? 1 + m_column_offset
        : location.column + m_column_offset;
    location.offset = Offset();

    return location;
}

}
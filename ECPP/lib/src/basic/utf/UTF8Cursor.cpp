#include <basic/utf/UTF8Cursor.h>

#include <basic/utf/Encodeing.h>

#include <stdexcept>

namespace BASIC::UTF {

const char8_t* UTF8Cursor::StartPtr() const noexcept {
    return m_data.data();
}

const char8_t* UTF8Cursor::EndPtr() const noexcept {
    return m_data.end();
}

const char8_t* UTF8Cursor::CurPtr() const noexcept {
    return StartPtr() + m_offset;
}

bool UTF8Cursor::Empty() const noexcept {
    return m_data.empty();
}

bool UTF8Cursor::AtEnd() const noexcept {
    return CurPtr() >= EndPtr();
}

bool UTF8Cursor::HasRemaining(const std::size_t size) const noexcept {
    return EndPtr() - CurPtr() >= size;
}

void UTF8Cursor::AdvanceOffset(const std::size_t offset) noexcept {
    m_offset += offset;
}

void UTF8Cursor::RetreatOffset(const std::size_t offset) noexcept {
    m_offset -= offset;
}

DecodedCodePoint UTF8Cursor::PeekAt(std::size_t cp_offset) const {
    std::size_t byte_offset = m_offset;

    while (byte_offset < m_data.size()) {
        DecodedCodePoint result;
        if (!DecodeUTF8(m_data.substr(byte_offset), result))
            throw std::runtime_error("UTF8Cursor::PeekAt: Invalid UTF-8 sequence");

        if (!cp_offset)
            return result;

        byte_offset += result.uint_length;
        cp_offset--;
    }

    throw std::out_of_range("UTF8Cursor::PeekAt: Code point offset exceeds input");
}

std::size_t UTF8Cursor::Offset() const noexcept {
    return m_offset;
}

}
#include <basic/source/source.h>

namespace BASIC {

const std::filesystem::path& Source::Path() const noexcept {
    return m_file_path;
}

std::size_t Source::Size() const noexcept {
    return m_text.size();
}

std::string_view Source::Text() const noexcept {
    return m_text;
}

}
#pragma once

#include <filesystem>
#include <string_view>

namespace BASIC {

class Source {
    public:
        Source(const Source&) = delete;
        Source& operator=(const Source&) = delete;
        Source(Source&&) = default;
        Source& operator=(Source&&) = default;

        const std::filesystem::path& Path() const noexcept;
        std::size_t Size() const noexcept;
        std::string_view Text() const noexcept;

    private:
        std::filesystem::path m_file_path;
        std::string m_text;
};

}
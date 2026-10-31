#pragma once

#include <utility/algorithms.h>
#include <basic/utf/utf_char.h>

#include <string_view>

namespace BASIC {

template <typename T>
bool Decode(const std::basic_string_view<T>, UTFChar&) noexcept;

template <typename T>
bool Encode(char32_t, std::basic_string<T>&);
    
}
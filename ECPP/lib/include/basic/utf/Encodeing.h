#pragma once

#include <basic/utf/DecodedCodePoint.h>

#include <string>
#include <string_view>

namespace BASIC::UTF {

bool DecodeUTF8(const std::u8string_view, DecodedCodePoint&) noexcept;

bool EncodeUTF8(const char32_t, std::u8string&);

}
#pragma once

#include <string_view>

namespace ECPP {

constexpr std::u8string_view ScriptStartDelimiter = u8"@ScriptStart";
constexpr std::u8string_view ScriptEndDelimiter = u8"@ScriptEnd";
    
}
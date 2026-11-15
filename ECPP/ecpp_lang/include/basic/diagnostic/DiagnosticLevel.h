#pragma once

#include <cstdint>

namespace LANG::BASIC::DIAGNOSTIC {

enum class DiagnosticLevel : std::uint8_t {
    WARNING,
    ERROR
};

}
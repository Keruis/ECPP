#include <basic/fail.h>

#include <format>
#include <stdexcept>

namespace BASIC {

[[noreturn]] void Fail(const std::string_view message, const Location loc) {
    throw std::runtime_error(std::format("{}:{} {}", loc.line, loc.column, message));
}

}
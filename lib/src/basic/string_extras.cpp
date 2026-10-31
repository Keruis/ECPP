#include <basic/string_extras.h>

#include <cassert>

namespace BASIC {

std::size_t ScanNumeric(const char8_t* start, const char8_t* end, std::size_t radix) noexcept {
    return ScanWhile<
        [](const char8_t c, std::size_t r)noexcept {
            return IsDigit(static_cast<char32_t>(c), r);
        }
    >(start, end, radix);
}

std::size_t IntegerStringToNumeric(const char8_t* start, const char8_t* end, bool& overflow, std::size_t max, std::size_t radix) noexcept {
    assert(ScanNumeric(start, end, radix) == end - start);

    std::size_t i = 0;

    while (start < end) {
        std::size_t digit = CharToDigit(static_cast<char32_t>(*start), radix);
        if (UTILITY::WouldOverflow(i, digit, max, radix)) {
            overflow = true;
            return max;
        }
        i = i * radix + digit;
        start++;
    }

    return i;
}

}
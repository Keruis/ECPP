#include <basic/string/StringAlgorithms.h>

#include <basic/ascii/CharClass.h>
#include <basic/integer/IntegerExtras.h>

namespace BASIC::STRING {

std::size_t ScanNumeric(const char8_t* const start, const char8_t* const end, const std::size_t radix) noexcept {
    return ScanWhile<[](const char8_t cp, std::size_t radix){
        return ASCII::IsDigit(static_cast<char32_t>(cp), radix);
    }>(start, end, radix);
}

std::size_t IntegerStringToNumeric(const char8_t* start, const char8_t* end, bool& overflow, const std::size_t max, const std::size_t radix) noexcept {
    std::size_t i = 0;

    while (start < end) {
        std::size_t digit = ASCII::DigitToValue(static_cast<char32_t>(*start), radix);
        if (INTEGER::WouldOverflow(i, digit, max, radix)) {
            overflow = true;
            return max;
        }
        i = i * radix + digit;
        start++;
    }

    return i;
}

}
#pragma once

#include <span>
#include <algorithm>

namespace BASIC::UNICODE {

class CodePointRangeSet {
    public:
        struct Ranges {
            char32_t start{};
            char32_t end{};
        };

    public:
        constexpr CodePointRangeSet(std::span<const Ranges> ranges)
            : ranges(ranges) 
        {}

        constexpr bool Contains(char32_t cp) const noexcept {
            auto it = std::ranges::lower_bound(ranges, cp, {}, &Ranges::end);
            return it != ranges.end() && it->start <= cp;
        }

    private:
        std::span<const Ranges> ranges;
};

}
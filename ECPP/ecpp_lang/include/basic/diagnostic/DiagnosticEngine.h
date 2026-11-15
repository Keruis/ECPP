#pragma once

#include <basic/source/SourceRange.h>
#include <basic/diagnostic/DiagnosticID.h>

#include <print>
#include <string>
#include <format>
#include <utility>
#include <iostream>

namespace LANG::BASIC::DIAGNOSTIC {

class DiagnosticEngine {
    public:
        template <DiagnosticID, typename... Args>
        static void Report(const ::BASIC::SOURCE::SourceRange&, Args&&...);

    private:
        template <DiagnosticID, typename... Args>
        static std::string FormatMessage(Args&&...);
};

template <DiagnosticID id, typename... Args>
void DiagnosticEngine::Report(const ::BASIC::SOURCE::SourceRange& range, Args&&... args) {
    std::string message = FormatMessage<id>(std::forward<Args>(args)...);
    std::println(std::cerr, "{}:{} {}", range.begin.line, range.begin.column, message);
}

template <DiagnosticID id, typename... Args>
std::string DiagnosticEngine::FormatMessage(Args&&... args) {
    constexpr auto definition = DiagnosticTable[std::to_underlying(id)];
    return std::format("{}: {}", definition.level == DiagnosticLevel::ERROR ? "error" : "warning", std::format(definition.format, std::forward<Args>(args)...));
}

}
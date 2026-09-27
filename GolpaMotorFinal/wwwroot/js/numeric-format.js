(function () {
    const selector = "[data-number-format]";

    function normalizeDigits(value) {
        return String(value ?? "")
            .replace(/[۰-۹]/g, function (digit) {
                return String("۰۱۲۳۴۵۶۷۸۹".indexOf(digit));
            })
            .replace(/[٠-٩]/g, function (digit) {
                return String("٠١٢٣٤٥٦٧٨٩".indexOf(digit));
            });
    }

    function rawNumber(value) {
        const normalized = normalizeDigits(value)
            .replace(/\u066B/g, ".")
            .replace(/[,\u066C\s]/g, "")
            .replace(/[^\d.-]/g, "");
        const negative = normalized.startsWith("-");
        const parts = normalized.replace(/-/g, "").split(".");
        const integer = parts.shift() || "";
        const fraction = parts.join("");
        return (negative ? "-" : "") + integer + (fraction ? "." + fraction : "");
    }

    function groupedNumber(value) {
        const raw = rawNumber(value);
        if (!raw || raw === "-" || raw === ".") return raw;
        const negative = raw.startsWith("-");
        const parts = raw.replace("-", "").split(".");
        const digits = (parts[0] || "0").replace(/^0+(?=\d)/, "");
        const grouped = digits.replace(/\B(?=(\d{3})+(?!\d))/g, ",");
        return (negative ? "-" : "") + grouped + (parts.length > 1 ? "." + parts[1] : "");
    }

    function formatInput(input) {
        if (!input || input.disabled) return;
        const cursorAtEnd = input.selectionStart === input.value.length;
        input.value = groupedNumber(input.value);
        if (cursorAtEnd && typeof input.setSelectionRange === "function") {
            input.setSelectionRange(input.value.length, input.value.length);
        }
    }

    window.initNumericInputs = function (root) {
        const scope = root || document;
        scope.querySelectorAll(selector).forEach(formatInput);
    };

    window.getRawFormattedNumber = rawNumber;
    window.formatGroupedNumber = groupedNumber;

    window.normalizeFormattedNumbers = function (root) {
        const scope = root || document;
        scope.querySelectorAll(selector).forEach(function (input) {
            input.value = rawNumber(input.value);
        });
    };

    document.addEventListener("input", function (event) {
        if (event.target.matches(selector))
            formatInput(event.target);
    });

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", function () {
            window.initNumericInputs(document);
        });
    } else {
        window.initNumericInputs(document);
    }
})();

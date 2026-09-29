(function (window, $) {
    "use strict";

    var persianZero = 0x06F0;
    var arabicZero = 0x0660;

    function toEnglishDigits(value) {
        var result = "";
        for (var i = 0; i < value.length; i++) {
            var code = value.charCodeAt(i);
            if (code >= persianZero && code <= persianZero + 9) {
                result += String.fromCharCode(48 + (code - persianZero));
            } else if (code >= arabicZero && code <= arabicZero + 9) {
                result += String.fromCharCode(48 + (code - arabicZero));
            } else {
                result += value.charAt(i);
            }
        }
        return result;
    }

    function normalize(value) {
        return toEnglishDigits(String(value || ""))
            .replace(/ /g, "")
            .replace(/-/g, "")
            .trim();
    }

    function isAsciiDigitString(value) {
        for (var i = 0; i < value.length; i++) {
            var code = value.charCodeAt(i);
            if (code < 48 || code > 57) {
                return false;
            }
        }
        return value.length > 0;
    }

    function isValidCard(value) {
        var number = normalize(value);
        if (number.length !== 16 || !isAsciiDigitString(number)) {
            return false;
        }

        var sum = 0;
        var doubleDigit = false;
        for (var index = number.length - 1; index >= 0; index--) {
            var digit = number.charCodeAt(index) - 48;
            if (doubleDigit) {
                digit *= 2;
                if (digit > 9) {
                    digit -= 9;
                }
            }
            sum += digit;
            doubleDigit = !doubleDigit;
        }
        return sum % 10 === 0;
    }

    function isValidSheba(value) {
        var sheba = normalize(value).toUpperCase();
        if (sheba.length !== 26 || sheba.substring(0, 2) !== "IR") {
            return false;
        }

        for (var i = 2; i < sheba.length; i++) {
            var digitCode = sheba.charCodeAt(i);
            if (digitCode < 48 || digitCode > 57) {
                return false;
            }
        }

        var rearranged = sheba.substring(4) + sheba.substring(0, 4);
        var remainder = 0;
        for (var j = 0; j < rearranged.length; j++) {
            var character = rearranged.charAt(j);
            var code = rearranged.charCodeAt(j);
            if (code >= 48 && code <= 57) {
                remainder = (remainder * 10 + (code - 48)) % 97;
            } else {
                var letterValue = character.charCodeAt(0) - 65 + 10;
                remainder = (remainder * 100 + letterValue) % 97;
            }
        }
        return remainder === 1;
    }

    function isValidAccount(value, minLength, maxLength) {
        var accountNumber = normalize(value);
        var min = minLength || 8;
        var max = maxLength || 50;
        return accountNumber.length >= min &&
            accountNumber.length <= max &&
            isAsciiDigitString(accountNumber);
    }

    function parseKind(params) {
        if (typeof params === "string") {
            return params;
        }
        if (params && params.kind) {
            return params.kind;
        }
        return "";
    }

    window.IranianBanking = {
        validate: function (kind, value, params) {
            var key = String(kind || "").toLowerCase();
            if (key === "card") {
                return isValidCard(value);
            }
            if (key === "sheba") {
                return isValidSheba(value);
            }
            if (key === "account") {
                var min = params && params.min ? parseInt(params.min, 10) : 8;
                var max = params && params.max ? parseInt(params.max, 10) : 50;
                return isValidAccount(value, min, max);
            }
            return false;
        }
    };

    if (!$ || !$.validator) {
        return;
    }

    $.validator.addMethod("bankingfield", function (value, element, params) {
        if (this.optional(element)) {
            return true;
        }
        if (!normalize(value)) {
            return true;
        }
        var kind = parseKind(params);
        return window.IranianBanking.validate(kind, value, params);
    });

    if ($.validator.unobtrusive && $.validator.unobtrusive.adapters) {
        $.validator.unobtrusive.adapters.add("bankingfield", ["kind", "min", "max"], function (options) {
            options.rules.bankingfield = {
                kind: options.params.kind,
                min: options.params.min,
                max: options.params.max
            };
            if (options.message) {
                options.messages.bankingfield = options.message;
            }
        });
    }
})(window, window.jQuery);

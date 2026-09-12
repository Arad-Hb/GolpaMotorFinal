(function () {
    const months = ["فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند"];
    const weekDays = ["ش", "ی", "د", "س", "چ", "پ", "ج"];

    function div(a, b) { return Math.trunc(a / b); }

    function jalaliToGregorian(jy, jm, jd) {
        jy = parseInt(jy, 10) - 979;
        jm = parseInt(jm, 10) - 1;
        jd = parseInt(jd, 10) - 1;
        var jDayNo = 365 * jy + div(jy, 33) * 8 + div((jy % 33) + 3, 4);
        for (var i = 0; i < jm; ++i) jDayNo += [31, 31, 31, 31, 31, 31, 30, 30, 30, 30, 30, 29][i];
        jDayNo += jd;
        var gDayNo = jDayNo + 79;
        var gy = 1600 + 400 * div(gDayNo, 146097);
        gDayNo = gDayNo % 146097;
        var leap = true;
        if (gDayNo >= 36525) {
            gDayNo--;
            gy += 100 * div(gDayNo, 36524);
            gDayNo = gDayNo % 36524;
            if (gDayNo >= 365) gDayNo++;
            else leap = false;
        }
        gy += 4 * div(gDayNo, 1461);
        gDayNo %= 1461;
        if (gDayNo >= 366) {
            leap = false;
            gDayNo--;
            gy += div(gDayNo, 365);
            gDayNo = gDayNo % 365;
        }
        var gd, gm;
        for (gm = 0; gDayNo >= [31, leap ? 29 : 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31][gm]; gm++)
            gDayNo -= [31, leap ? 29 : 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31][gm];
        gd = gDayNo + 1;
        return { gy: gy, gm: gm + 1, gd: gd };
    }

    function gregorianToJalali(gy, gm, gd) {
        gy = parseInt(gy, 10) - 1600;
        gm = parseInt(gm, 10) - 1;
        gd = parseInt(gd, 10) - 1;
        var gDayNo = 365 * gy + div(gy + 3, 4) - div(gy + 99, 100) + div(gy + 399, 400);
        for (var i = 0; i < gm; ++i) gDayNo += [31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31][i];
        if (gm > 1 && ((gy % 4 === 0 && gy % 100 !== 0) || (gy % 400 === 0))) gDayNo++;
        gDayNo += gd;
        var jDayNo = gDayNo - 79;
        var jNp = div(jDayNo, 12053);
        jDayNo %= 12053;
        var jy = 979 + 33 * jNp + 4 * div(jDayNo, 1461);
        jDayNo %= 1461;
        if (jDayNo >= 366) {
            jy += div(jDayNo - 1, 365);
            jDayNo = (jDayNo - 1) % 365;
        }
        var jm;
        var i;
        for (i = 0; i < 11 && jDayNo >= [31, 31, 31, 31, 31, 31, 30, 30, 30, 30, 30, 29][i]; ++i)
            jDayNo -= [31, 31, 31, 31, 31, 31, 30, 30, 30, 30, 30, 29][i];
        jm = i + 1;
        var jd = jDayNo + 1;
        return { jy: jy, jm: jm, jd: jd };
    }

    function pad(n) { return (n < 10 ? "0" : "") + n; }

    function jalaliLeap(jy) {
        return (((jy - 474) % 2820) + 474 + 38) * 682 % 2816 < 682;
    }

    function daysInMonth(jy, jm) {
        if (jm <= 6) return 31;
        if (jm <= 11) return 30;
        return jalaliLeap(jy) ? 30 : 29;
    }

    function parseJalali(val) {
        var parts = (val || "").replace(/-/g, "/").split("/");
        if (parts.length !== 3) return null;
        var y = parseInt(parts[0], 10), m = parseInt(parts[1], 10), d = parseInt(parts[2], 10);
        if (!y || !m || !d) return null;
        return { jy: y, jm: m, jd: d };
    }

    function formatJ(j) {
        if (!j) return "";
        return j.jy + "/" + pad(j.jm) + "/" + pad(j.jd);
    }

    function sameDay(a, b) {
        return a && b && a.jy === b.jy && a.jm === b.jm && a.jd === b.jd;
    }

    function jToDate(j) {
        var g = jalaliToGregorian(j.jy, j.jm, j.jd);
        return new Date(g.gy, g.gm - 1, g.gd);
    }

    function dateToJ(d) {
        return gregorianToJalali(d.getFullYear(), d.getMonth() + 1, d.getDate());
    }

    function todayJ() {
        return dateToJ(new Date());
    }

    function addDays(j, n) {
        var d = jToDate(j);
        d.setDate(d.getDate() + n);
        return dateToJ(d);
    }

    function addMonths(j, n) {
        var m = j.jm + n;
        var y = j.jy;
        while (m < 1) { m += 12; y--; }
        while (m > 12) { m -= 12; y++; }
        return { jy: y, jm: m, jd: Math.min(j.jd, daysInMonth(y, m)) };
    }

    function cmpJ(a, b) {
        if (a.jy !== b.jy) return a.jy - b.jy;
        if (a.jm !== b.jm) return a.jm - b.jm;
        return a.jd - b.jd;
    }

    function inRange(day, from, to) {
        if (!from || !to) return false;
        return cmpJ(day, from) >= 0 && cmpJ(day, to) <= 0;
    }

    function monthStartOffset(jy, jm) {
        var gDay = jToDate({ jy: jy, jm: jm, jd: 1 }).getDay();
        return (gDay + 1) % 7;
    }

    function getPresets() {
        var today = todayJ();
        var yesterday = addDays(today, -1);
        var gDow = jToDate(today).getDay();
        var daysSinceSat = (gDow + 1) % 7;
        var thisWeekStart = addDays(today, -daysSinceSat);
        var lastWeekStart = addDays(thisWeekStart, -7);
        var lastWeekEnd = addDays(thisWeekStart, -1);
        var prev = today.jm === 1 ? { jy: today.jy - 1, jm: 12 } : { jy: today.jy, jm: today.jm - 1 };
        var lastMonthStart = { jy: prev.jy, jm: prev.jm, jd: 1 };
        var lastMonthEnd = { jy: prev.jy, jm: prev.jm, jd: daysInMonth(prev.jy, prev.jm) };
        return [
            { key: "today", label: "امروز", from: today, to: today },
            { key: "yesterday", label: "دیروز", from: yesterday, to: yesterday },
            { key: "lastWeek", label: "هفته گذشته", from: lastWeekStart, to: lastWeekEnd },
            { key: "lastMonth", label: "ماه گذشته", from: lastMonthStart, to: lastMonthEnd },
            { key: "last3Months", label: "سه ماه گذشته", from: addMonths(today, -3), to: today },
            { key: "lastYear", label: "یک سال گذشته", from: addMonths(today, -12), to: today }
        ];
    }

    function presetLabelFor(from, to) {
        if (!from || !to) return "";
        var list = getPresets();
        for (var i = 0; i < list.length; i++) {
            if (sameDay(list[i].from, from) && sameDay(list[i].to, to)) return list[i].label;
        }
        return formatJ(from) + " – " + formatJ(to);
    }

    function closePickers() {
        $(".jdp-popup, .date-range-popup, .num-range-popup, .filter-select-popup").remove();
        $(".date-range.is-open, .num-range.is-open, .filter-select.is-open").removeClass("is-open");
    }

    function renderSinglePicker($input) {
        closePickers();
        var current = parseJalali($input.val());
        var today = todayJ();
        var y = current ? current.jy : today.jy;
        var m = current ? current.jm : today.jm;
        var popup = $('<div class="jdp-popup"></div>');
        function draw() {
            var dim = daysInMonth(y, m);
            var offset = monthStartOffset(y, m);
            var html = '<div class="jdp-head"><button type="button" class="jdp-nav" data-dir="-1">&lt;</button><span>' + months[m - 1] + " " + y + '</span><button type="button" class="jdp-nav" data-dir="1">&gt;</button></div>';
            html += '<div class="jdp-week">';
            weekDays.forEach(function (w) { html += "<span>" + w + "</span>"; });
            html += '</div><div class="jdp-grid">';
            for (var i = 0; i < offset; i++) html += "<span></span>";
            for (var d = 1; d <= dim; d++) {
                var isToday = y === today.jy && m === today.jm && d === today.jd;
                var selected = current && current.jy === y && current.jm === m && current.jd === d;
                html += '<button type="button" class="jdp-day' + (isToday ? " is-today" : "") + (selected ? " is-selected" : "") + '" data-d="' + d + '">' + d + "</button>";
            }
            html += "</div>";
            popup.html(html);
        }
        draw();
        var offset = $input.offset();
        popup.css({ top: offset.top + $input.outerHeight() + 4, left: offset.left });
        $("body").append(popup);
        popup.on("click", ".jdp-nav", function (e) {
            e.stopPropagation();
            m += parseInt($(this).data("dir"), 10);
            if (m < 1) { m = 12; y--; }
            if (m > 12) { m = 1; y++; }
            draw();
        });
        popup.on("click", ".jdp-day", function () {
            $input.val(formatJ({ jy: y, jm: m, jd: parseInt($(this).data("d"), 10) })).trigger("change");
            closePickers();
        });
    }

    function emptyLabel($wrap) {
        return $wrap.attr("data-empty-label") || "انتخاب تاریخ";
    }

    function updateTrigger($wrap) {
        var $from = $wrap.find(".js-date-from");
        var $to = $wrap.find(".js-date-to");
        var from = parseJalali($from.val());
        var to = parseJalali($to.val());
        var text = (!from || !to) ? emptyLabel($wrap) : presetLabelFor(from, to);
        $wrap.find(".date-range__label").text(text);
        $wrap.toggleClass("has-value", !!(from && to));
    }

    function applyRange($wrap, from, to, triggerChange) {
        if (from && to && cmpJ(from, to) > 0) {
            var tmp = from;
            from = to;
            to = tmp;
        }
        $wrap.find(".js-date-from").val(from ? formatJ(from) : "");
        $wrap.find(".js-date-to").val(to ? formatJ(to) : "");
        updateTrigger($wrap);
        if (triggerChange) {
            $wrap.find(".js-date-to").trigger("change");
        }
    }

    function calendarHtml(y, m, from, to) {
        var today = todayJ();
        var dim = daysInMonth(y, m);
        var offset = monthStartOffset(y, m);
        var html = '<div class="date-range-cal" data-y="' + y + '" data-m="' + m + '">';
        html += '<div class="date-range-cal__head"><button type="button" class="date-range-cal__nav" data-dir="-1" aria-label="ماه قبل"><i class="fa-solid fa-chevron-right"></i></button>';
        html += "<strong>" + months[m - 1] + " " + y + "</strong>";
        html += '<button type="button" class="date-range-cal__nav" data-dir="1" aria-label="ماه بعد"><i class="fa-solid fa-chevron-left"></i></button></div>';
        html += '<div class="date-range-cal__week">';
        weekDays.forEach(function (w) { html += "<span>" + w + "</span>"; });
        html += "</div><div class=\"date-range-cal__grid\">";
        for (var i = 0; i < offset; i++) html += "<span class=\"date-range-cal__pad\"></span>";
        var rangeFrom = from;
        var rangeEnd = to;
        if (rangeFrom && rangeEnd && cmpJ(rangeFrom, rangeEnd) > 0) {
            var sw = rangeFrom;
            rangeFrom = rangeEnd;
            rangeEnd = sw;
        }
        for (var d = 1; d <= dim; d++) {
            var day = { jy: y, jm: m, jd: d };
            var cls = "date-range-cal__day";
            if (sameDay(day, today)) cls += " is-today";
            if (from && sameDay(day, from)) cls += " is-start";
            if (to && sameDay(day, to)) cls += " is-end";
            if (from && !to && sameDay(day, from)) cls += " is-end";
            if (rangeFrom && rangeEnd && inRange(day, rangeFrom, rangeEnd)) cls += " is-in-range";
            html += '<button type="button" class="' + cls + '" data-d="' + d + '">' + d + "</button>";
        }
        html += "</div></div>";
        return html;
    }

    function nextMonth(y, m) {
        m++;
        if (m > 12) { m = 1; y++; }
        return { y: y, m: m };
    }

    function prevMonth(y, m) {
        m--;
        if (m < 1) { m = 12; y--; }
        return { y: y, m: m };
    }

    function openRangePopup($wrap) {
        closePickers();
        $wrap.addClass("is-open");
        var from = parseJalali($wrap.find(".js-date-from").val());
        var to = parseJalali($wrap.find(".js-date-to").val());
        var pickStart = from;
        var pickEnd = to;
        var view = from || todayJ();
        var left = { y: view.jy, m: view.jm };
        var right = nextMonth(left.y, left.m);
        if (to && (to.jy !== view.jy || to.jm !== view.jm)) {
            right = { y: to.jy, m: to.jm };
            if (right.y === left.y && right.m === left.m) right = nextMonth(left.y, left.m);
        }
        var popup = $('<div class="date-range-popup" dir="rtl"></div>');

        function draw() {
            var presets = getPresets();
            var html = '<div class="date-range-popup__presets">';
            presets.forEach(function (p) {
                var active = pickStart && pickEnd && sameDay(p.from, pickStart) && sameDay(p.to, pickEnd);
                html += '<button type="button" class="date-range-popup__preset' + (active ? " is-active" : "") + '" data-key="' + p.key + '">' + p.label + "</button>";
            });
            html += "</div>";
            html += '<div class="date-range-popup__cals">';
            html += calendarHtml(left.y, left.m, pickStart, pickEnd);
            html += calendarHtml(right.y, right.m, pickStart, pickEnd);
            html += "</div>";
            popup.html(html);
        }

        function position() {
            var rect = $wrap.find(".date-range__trigger")[0].getBoundingClientRect();
            var pw = popup.outerWidth();
            var ph = popup.outerHeight();
            var top = rect.bottom + 8;
            if (top + ph > window.innerHeight - 8) top = Math.max(8, rect.top - ph - 8);
            var rightPos = window.innerWidth - rect.right;
            if (rect.right - pw < 8) rightPos = Math.max(8, window.innerWidth - pw - 8);
            popup.css({ top: top + "px", right: rightPos + "px", left: "auto" });
        }

        draw();
        $("body").append(popup);
        position();

        popup.on("click", ".date-range-popup__preset", function (e) {
            e.stopPropagation();
            var key = $(this).data("key");
            var p = getPresets().filter(function (x) { return x.key === key; })[0];
            if (!p) return;
            pickStart = p.from;
            pickEnd = p.to;
            left = { y: pickStart.jy, m: pickStart.jm };
            right = { y: pickEnd.jy, m: pickEnd.jm };
            if (right.y === left.y && right.m === left.m) right = nextMonth(left.y, left.m);
            applyRange($wrap, pickStart, pickEnd, true);
            closePickers();
        });

        popup.on("click", ".date-range-cal__nav", function (e) {
            e.stopPropagation();
            var $cal = $(this).closest(".date-range-cal");
            var isLeft = $cal.index() === 0;
            var dir = parseInt($(this).data("dir"), 10);
            if (isLeft) {
                left = dir < 0 ? prevMonth(left.y, left.m) : nextMonth(left.y, left.m);
                var leftIdx = left.y * 12 + left.m;
                var rightIdx = right.y * 12 + right.m;
                if (leftIdx >= rightIdx) right = nextMonth(left.y, left.m);
            } else {
                right = dir < 0 ? prevMonth(right.y, right.m) : nextMonth(right.y, right.m);
                var lIdx = left.y * 12 + left.m;
                var rIdx = right.y * 12 + right.m;
                if (rIdx <= lIdx) left = prevMonth(right.y, right.m);
            }
            draw();
        });

        popup.on("click", ".date-range-cal__day", function (e) {
            e.stopPropagation();
            var $cal = $(this).closest(".date-range-cal");
            var day = { jy: parseInt($cal.data("y"), 10), jm: parseInt($cal.data("m"), 10), jd: parseInt($(this).data("d"), 10) };
            if (!pickStart || pickEnd) {
                pickStart = day;
                pickEnd = null;
                draw();
                return;
            }
            pickEnd = day;
            applyRange($wrap, pickStart, pickEnd, true);
            closePickers();
        });

        $(window).off("resize.dateRange").on("resize.dateRange", position);
    }

    function shiftRange($wrap, dir) {
        var from = parseJalali($wrap.find(".js-date-from").val());
        var to = parseJalali($wrap.find(".js-date-to").val());
        if (!from || !to) {
            var t = todayJ();
            applyRange($wrap, t, t, true);
            return;
        }
        var span = Math.round((jToDate(to) - jToDate(from)) / 86400000) || 1;
        applyRange($wrap, addDays(from, dir * span), addDays(to, dir * span), true);
    }

    function initDateRange($wrap) {
        if ($wrap.data("rangeReady")) return;
        $wrap.data("rangeReady", true);
        $wrap.addClass("date-range");
        var $inputs = $wrap.find("input.js-jalali");
        if ($inputs.length < 2) return;
        $inputs.eq(0).addClass("js-date-from js-date-range-input").attr({ tabindex: "-1", "aria-hidden": "true" });
        $inputs.eq(1).addClass("js-date-to js-date-range-input").attr({ tabindex: "-1", "aria-hidden": "true" });
        if (!$wrap.find(".date-range__trigger").length) {
            $wrap.prepend(
                '<div class="date-range__control">' +
                '<button type="button" class="date-range__nav" data-dir="-1" aria-label="بازه قبلی"><i class="fa-solid fa-chevron-right"></i></button>' +
                '<button type="button" class="date-range__trigger" aria-haspopup="dialog">' +
                '<i class="fa-solid fa-calendar-days"></i>' +
                '<span class="date-range__label"></span>' +
                "</button>" +
                '<button type="button" class="date-range__nav" data-dir="1" aria-label="بازه بعدی"><i class="fa-solid fa-chevron-left"></i></button>' +
                "</div>"
            );
        }
        updateTrigger($wrap);
    }

    function wrapDatePairs(root) {
        var $root = $(root || document);
        $root.find(".js-date-range").each(function () {
            initDateRange($(this));
        });
        var leftover = $root.find("input.js-jalali").not(".js-date-range-input").get();
        for (var i = 0; i < leftover.length - 1; i++) {
            var $a = $(leftover[i]);
            var $b = $(leftover[i + 1]);
            if (!$a.length || !$b.length) continue;
            if ($a.parent()[0] !== $b.parent()[0]) continue;
            if ($a.closest(".date-range, .js-date-range").length) continue;
            var $w = $('<div class="date-range js-date-range" data-empty-label="انتخاب تاریخ"></div>');
            $a.before($w);
            $w.append($a).append($b);
            initDateRange($w);
            i++;
        }
    }

    function bootDateRanges() {
        wrapDatePairs(document);
        wrapNumPairs(document);
        wrapFilterSelects(document);
    }
    $(bootDateRanges);
    if (document.readyState !== "loading") bootDateRanges();

    function numEmptyLabel($wrap) {
        return $wrap.attr("data-empty-label") || "بازه عدد";
    }

    function parseNum(val) {
        if (val === null || val === undefined || String(val).trim() === "") return null;
        var n = Number(val);
        return isNaN(n) ? null : n;
    }

    function numLabel(from, to, empty) {
        if (from === null && to === null) return empty;
        if (from !== null && to !== null) return from + " – " + to;
        if (from !== null) return "از " + from;
        return "تا " + to;
    }

    function updateNumTrigger($wrap) {
        var from = parseNum($wrap.find(".js-num-from").val());
        var to = parseNum($wrap.find(".js-num-to").val());
        $wrap.find(".num-range__label").text(numLabel(from, to, numEmptyLabel($wrap)));
        $wrap.toggleClass("has-value", from !== null || to !== null);
    }

    function applyNumRange($wrap, from, to, triggerChange) {
        if (from !== null && to !== null && from > to) {
            var tmp = from;
            from = to;
            to = tmp;
        }
        $wrap.find(".js-num-from").val(from === null ? "" : from);
        $wrap.find(".js-num-to").val(to === null ? "" : to);
        updateNumTrigger($wrap);
        if (triggerChange) {
            $wrap.find(".js-num-to").trigger("change");
        }
    }

    function numStep($wrap) {
        var step = parseNum($wrap.find(".js-num-from").attr("step"));
        return step && step > 0 ? step : 1;
    }

    function initNumRange($wrap) {
        if ($wrap.data("numReady")) return;
        $wrap.data("numReady", true);
        $wrap.addClass("num-range");
        var $inputs = $wrap.find("input[type=number]");
        if ($inputs.length < 2) return;
        $inputs.eq(0).addClass("js-num-from js-num-range-input").attr({ tabindex: "-1", "aria-hidden": "true" });
        $inputs.eq(1).addClass("js-num-to js-num-range-input").attr({ tabindex: "-1", "aria-hidden": "true" });
        if (!$wrap.find(".num-range__trigger").length) {
            $wrap.prepend(
                '<div class="num-range__control">' +
                '<button type="button" class="num-range__nav" data-dir="-1" aria-label="بازه قبلی"><i class="fa-solid fa-chevron-right"></i></button>' +
                '<button type="button" class="num-range__trigger" aria-haspopup="dialog">' +
                '<i class="fa-solid fa-sliders"></i>' +
                '<span class="num-range__label"></span>' +
                "</button>" +
                '<button type="button" class="num-range__nav" data-dir="1" aria-label="بازه بعدی"><i class="fa-solid fa-chevron-left"></i></button>' +
                "</div>"
            );
        }
        updateNumTrigger($wrap);
    }

    function wrapNumPairs(root) {
        var $root = $(root || document);
        $root.find(".js-num-range, .filter-range").each(function () {
            var $box = $(this);
            if ($box.find("input[type=number]").length >= 2) initNumRange($box);
        });
    }

    function positionFixedPopup($anchor, popup) {
        var rect = $anchor[0].getBoundingClientRect();
        var pw = popup.outerWidth();
        var ph = popup.outerHeight();
        var top = rect.bottom + 8;
        if (top + ph > window.innerHeight - 8) top = Math.max(8, rect.top - ph - 8);
        var rightPos = window.innerWidth - rect.right;
        if (rect.right - pw < 8) rightPos = Math.max(8, window.innerWidth - pw - 8);
        popup.css({ top: top + "px", right: rightPos + "px", left: "auto" });
    }

    function openNumPopup($wrap) {
        closePickers();
        $wrap.addClass("is-open");
        var from = $wrap.find(".js-num-from").val();
        var to = $wrap.find(".js-num-to").val();
        var step = numStep($wrap);
        var popup = $(
            '<div class="num-range-popup" dir="rtl">' +
            '<div class="num-range-popup__fields">' +
            '<label class="num-range-popup__field"><span>حداقل</span><input type="number" class="num-range-popup__from" step="' + step + '" inputmode="numeric" /></label>' +
            '<span class="num-range-popup__sep">تا</span>' +
            '<label class="num-range-popup__field"><span>حداکثر</span><input type="number" class="num-range-popup__to" step="' + step + '" inputmode="numeric" /></label>' +
            "</div>" +
            '<div class="num-range-popup__actions">' +
            '<button type="button" class="num-range-popup__clear">پاک کردن</button>' +
            '<button type="button" class="num-range-popup__apply">اعمال</button>' +
            "</div></div>"
        );
        popup.find(".num-range-popup__from").val(from);
        popup.find(".num-range-popup__to").val(to);
        $("body").append(popup);
        positionFixedPopup($wrap.find(".num-range__trigger"), popup);
        popup.find(".num-range-popup__from").trigger("focus").trigger("select");

        function commit(apply) {
            var nextFrom = parseNum(popup.find(".num-range-popup__from").val());
            var nextTo = parseNum(popup.find(".num-range-popup__to").val());
            applyNumRange($wrap, nextFrom, nextTo, apply);
            closePickers();
        }

        popup.on("click", ".num-range-popup__apply", function (e) {
            e.stopPropagation();
            commit(true);
        });
        popup.on("click", ".num-range-popup__clear", function (e) {
            e.stopPropagation();
            applyNumRange($wrap, null, null, true);
            closePickers();
        });
        popup.on("keydown", "input", function (e) {
            if (e.key === "Enter") {
                e.preventDefault();
                commit(true);
            }
        });
        $(window).off("resize.numRange").on("resize.numRange", function () {
            positionFixedPopup($wrap.find(".num-range__trigger"), popup);
        });
    }

    function shiftNumRange($wrap, dir) {
        var from = parseNum($wrap.find(".js-num-from").val());
        var to = parseNum($wrap.find(".js-num-to").val());
        var step = numStep($wrap) * dir;
        if (from === null && to === null) return;
        if (from !== null && to !== null) {
            var span = Math.abs(to - from) || Math.abs(step);
            applyNumRange($wrap, from + dir * span, to + dir * span, true);
            return;
        }
        if (from !== null) applyNumRange($wrap, from + step, null, true);
        else applyNumRange($wrap, null, to + step, true);
    }

    function updateFilterSelect($wrap) {
        var $sel = $wrap.find("select");
        var $opt = $sel.find("option:selected");
        if (!$opt.length) $opt = $sel.find("option").first();
        $wrap.find(".filter-select__label").text($.trim($opt.text() || ""));
        $wrap.toggleClass("has-value", !!$sel.val());
    }

    function initFilterSelect($select) {
        if ($select.data("selectReady") || $select.closest(".filter-select").length) return;
        $select.data("selectReady", true);
        var $wrap = $('<div class="filter-select"></div>');
        $select.before($wrap);
        $wrap.append($select);
        $wrap.append(
            '<button type="button" class="filter-select__trigger" aria-haspopup="listbox">' +
            '<span class="filter-select__label"></span>' +
            '<i class="fa-solid fa-chevron-down"></i>' +
            "</button>"
        );
        if ($select.prop("disabled")) {
            $wrap.addClass("is-disabled").find(".filter-select__trigger").prop("disabled", true);
        }
        if ($select.hasClass("form-select-sm") || $select.hasClass("crud-page-size")) {
            $wrap.addClass("filter-select--sm");
        }
        updateFilterSelect($wrap);
    }

    function wrapFilterSelects(root) {
        $(root || document).find("select.form-select, select.form-control").each(function () {
            var $select = $(this);
            if (this.multiple || ($select.attr("size") && $select.attr("size") !== "1")) return;
            if ($select.closest(".swal2-container, .filter-select-popup").length) return;
            initFilterSelect($select);
        });
    }

    window.initAdminSelects = wrapFilterSelects;
    window.refreshFilterSelect = function (el) {
        var $wrap = $(el).closest(".filter-select");
        if ($wrap.length) updateFilterSelect($wrap);
    };

    if (window.MutationObserver) {
        var selectScanQueued = false;
        var selectObserver = new MutationObserver(function () {
            if (selectScanQueued) return;
            selectScanQueued = true;
            setTimeout(function () {
                selectScanQueued = false;
                wrapFilterSelects(document);
            }, 0);
        });
        $(function () {
            if (document.body) {
                selectObserver.observe(document.body, { childList: true, subtree: true });
            }
        });
    }

    function openFilterSelect($wrap) {
        closePickers();
        $wrap.addClass("is-open");
        var $sel = $wrap.find("select");
        var popup = $('<div class="filter-select-popup" dir="rtl" role="listbox"></div>');
        $sel.find("option").each(function () {
            var $opt = $(this);
            var active = this.selected ? " is-active" : "";
            popup.append(
                $('<button type="button" class="filter-select-popup__opt' + active + '"></button>')
                    .attr("data-value", $opt.attr("value") || "")
                    .text($.trim($opt.text() || ""))
            );
        });
        $("body").append(popup);
        var minW = Math.max($wrap.outerWidth(), 160);
        popup.css("min-width", minW + "px");
        positionFixedPopup($wrap.find(".filter-select__trigger"), popup);

        popup.on("click", ".filter-select-popup__opt", function (e) {
            e.stopPropagation();
            var val = $(this).attr("data-value");
            $sel.val(val).trigger("change");
            $wrap.removeClass("is-invalid");
            updateFilterSelect($wrap);
            closePickers();
        });
        $(window).off("resize.filterSelect").on("resize.filterSelect", function () {
            positionFixedPopup($wrap.find(".filter-select__trigger"), popup);
        });
    }

    $(document).on("click", ".filter-select__trigger", function (e) {
        e.preventDefault();
        e.stopPropagation();
        var $wrap = $(this).closest(".filter-select");
        if ($wrap.hasClass("is-open")) closePickers();
        else openFilterSelect($wrap);
    });

    $(document).on("invalid", "select", function () {
        $(this).closest(".filter-select").addClass("is-invalid");
    });

    $(document).on("click", ".num-range__trigger", function (e) {
        e.preventDefault();
        e.stopPropagation();
        var $wrap = $(this).closest(".num-range");
        if ($wrap.hasClass("is-open")) closePickers();
        else openNumPopup($wrap);
    });

    $(document).on("click", ".num-range__nav", function (e) {
        e.preventDefault();
        e.stopPropagation();
        shiftNumRange($(this).closest(".num-range"), parseInt($(this).data("dir"), 10));
    });

    $(document).on("focus click", ".js-jalali:not(.js-date-range-input)", function (e) {
        e.stopPropagation();
        renderSinglePicker($(this));
    });

    $(document).on("click", ".date-range__trigger", function (e) {
        e.preventDefault();
        e.stopPropagation();
        var $wrap = $(this).closest(".date-range");
        if ($wrap.hasClass("is-open")) closePickers();
        else openRangePopup($wrap);
    });

    $(document).on("click", ".date-range__nav", function (e) {
        e.preventDefault();
        e.stopPropagation();
        shiftRange($(this).closest(".date-range"), parseInt($(this).data("dir"), 10));
    });

    $(document).on("click", function () { closePickers(); });
    $(document).on("click", ".jdp-popup, .js-jalali, .date-range-popup, .date-range, .num-range-popup, .num-range, .filter-select-popup, .filter-select", function (e) { e.stopPropagation(); });

    function serializeBar($bar) {
        var data = {};
        $bar.find("input, select").each(function () {
            var name = this.name;
            if (!name || this.disabled) return;
            var val = $(this).val();
            if (val !== null && val !== "") data[name] = val;
        });
        return data;
    }

    function applyFilter($bar, page) {
        var url = $bar.attr("data-url");
        var target = $bar.attr("data-target");
        if (!url || !target) return;
        var data = serializeBar($bar);
        data.pageIndex = page || 0;
        var qs = $.param(data);
        $(target).load(url + (url.indexOf("?") >= 0 ? "&" : "?") + qs, function () {
            wrapFilterSelects(target);
        });
    }

    window.applyTableFilter = applyFilter;

    $(document).on("submit", "[data-filter-bar]", function (e) {
        e.preventDefault();
        applyFilter($(this), 0);
    });

    $(document).on("click", "[data-filter-search]", function (e) {
        e.preventDefault();
        applyFilter($(this).closest("[data-filter-bar]"), 0);
    });

    $(document).on("keydown", "[data-filter-bar] .filter-search__input", function (e) {
        if (e.key === "Enter") {
            e.preventDefault();
            applyFilter($(this).closest("[data-filter-bar]"), 0);
        }
    });

    $(document).on("change", "[data-filter-bar] select, [data-filter-bar] input[type=number], [data-filter-bar] .js-jalali", function () {
        var $bar = $(this).closest("[data-filter-bar]");
        if (this.name === "ProvinceID") return;
        if ($(this).closest(".num-range-popup").length) return;
        applyFilter($bar, 0);
    });

    $(document).on("click", "[data-filter-reset]", function (e) {
        e.preventDefault();
        var $bar = $(this).closest("[data-filter-bar]");
        $bar.find("input, select").each(function () {
            if (this.tagName === "SELECT") this.selectedIndex = 0;
            else $(this).val("");
        });
        var $city = $bar.find("[name=CityID]");
        if ($city.length) {
            $city.html('<option value="">همه شهرها</option>');
        }
        $bar.find(".date-range").each(function () { updateTrigger($(this)); });
        $bar.find(".num-range").each(function () { updateNumTrigger($(this)); });
        $bar.find(".filter-select").each(function () { updateFilterSelect($(this)); });
        applyFilter($bar, 0);
    });

    $(document).on("change", "[data-filter-bar] [name=ProvinceID]", function () {
        var $bar = $(this).closest("[data-filter-bar]");
        var $city = $bar.find("[name=CityID]");
        var id = $(this).val();
        $city.html('<option value="">همه شهرها</option>');
        updateFilterSelect($city.closest(".filter-select"));
        if (id) {
            $.get("/UserManagement/GetCitiesByProvince", { provinceId: id }, function (res) {
                var list = res && res.data ? res.data : (Array.isArray(res) ? res : []);
                list.forEach(function (c) {
                    $city.append($("<option>").val(c.cityID || c.CityID).text(c.name || c.Name));
                });
                updateFilterSelect($city.closest(".filter-select"));
                applyFilter($bar, 0);
            });
        } else {
            updateFilterSelect($city.closest(".filter-select"));
            applyFilter($bar, 0);
        }
    });
})();

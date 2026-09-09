(function () {
    const months = ["فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند"];

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

    function parseJalali(val) {
        var parts = (val || "").replace(/-/g, "/").split("/");
        if (parts.length !== 3) return null;
        var y = parseInt(parts[0], 10), m = parseInt(parts[1], 10), d = parseInt(parts[2], 10);
        if (!y || !m || !d) return null;
        return { jy: y, jm: m, jd: d };
    }

    function closePickers() {
        $(".jdp-popup").remove();
    }

    function renderPicker($input) {
        closePickers();
        var current = parseJalali($input.val());
        var now = new Date();
        var todayJ = gregorianToJalali(now.getFullYear(), now.getMonth() + 1, now.getDate());
        var y = current ? current.jy : todayJ.jy;
        var m = current ? current.jm : todayJ.jm;
        var popup = $('<div class="jdp-popup"></div>');
        function draw() {
            var daysInMonth = m <= 6 ? 31 : (m <= 11 ? 30 : 29);
            var html = '<div class="jdp-head"><button type="button" class="jdp-nav" data-dir="-1">&lt;</button><span>' + months[m - 1] + " " + y + '</span><button type="button" class="jdp-nav" data-dir="1">&gt;</button></div><div class="jdp-grid">';
            for (var d = 1; d <= daysInMonth; d++) {
                html += '<button type="button" class="jdp-day" data-d="' + d + '">' + d + "</button>";
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
            $input.val(y + "/" + pad(m) + "/" + pad(parseInt($(this).data("d"), 10))).trigger("change");
            closePickers();
        });
    }

    $(document).on("focus click", ".js-jalali", function (e) {
        e.stopPropagation();
        renderPicker($(this));
    });
    $(document).on("click", function () { closePickers(); });
    $(document).on("click", ".jdp-popup, .js-jalali", function (e) { e.stopPropagation(); });

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
        $(target).load(url + (url.indexOf("?") >= 0 ? "&" : "?") + qs);
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
        applyFilter($bar, 0);
    });

    $(document).on("change", "[data-filter-bar] [name=ProvinceID]", function () {
        var $bar = $(this).closest("[data-filter-bar]");
        var $city = $bar.find("[name=CityID]");
        var id = $(this).val();
        $city.html('<option value="">همه شهرها</option>');
        if (id) {
            $.get("/UserManagement/GetCitiesByProvince", { provinceId: id }, function (res) {
                var list = res && res.data ? res.data : (Array.isArray(res) ? res : []);
                list.forEach(function (c) {
                    $city.append($("<option>").val(c.cityID || c.CityID).text(c.name || c.Name));
                });
                applyFilter($bar, 0);
            });
        } else {
            applyFilter($bar, 0);
        }
    });
})();

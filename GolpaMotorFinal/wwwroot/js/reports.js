(function () {
    "use strict";

    var root = $("[data-report-page]");

    if (!root.length) {
        return;
    }

    var bar = root.find("[data-filter-bar]").first();

    // Prevent the shared filter script from starting a second request.
    bar.attr("data-autoload", "false");

    var pendingRequest = null;
    var requestVersion = 0;
    var restoring = false;

    function owns(filterBar) {
        return filterBar &&
            filterBar.length &&
            filterBar.closest("[data-report-page]").length > 0;
    }

    function readFields(filterBar) {
        var parameters = new URLSearchParams();

        filterBar.find("input[name], select[name]").each(function () {
            if (this.disabled) {
                return;
            }

            if ((this.type === "checkbox" || this.type === "radio") &&
                !this.checked) {
                return;
            }

            var value = $(this).val();

            if (value !== null && value !== "") {
                parameters.set(this.name, String(value));
            }
        });

        return parameters;
    }

    function getPageIndex(parameters) {
        var pageIndex = 0;

        parameters.forEach(function (value, key) {
            if (key.toLowerCase() === "pageindex") {
                var parsed = Number(value);

                if (Number.isInteger(parsed) && parsed >= 0) {
                    pageIndex = parsed;
                }
            }
        });

        return pageIndex;
    }

    function updateAddress(parameters, replace) {
        var pathname = root.attr("data-report-page");
        var query = parameters.toString();

        var address = pathname + (query ? "?" + query : "");

        var currentAddress =
            window.location.pathname + window.location.search;

        if (address === currentAddress) {
            return;
        }

        if (replace) {
            window.history.replaceState(null, "", address);
        } else {
            window.history.pushState(null, "", address);
        }
    }

    function refreshLabels(filterBar) {
        if (window.refreshReportFilterLabels) {
            window.refreshReportFilterLabels(filterBar);
        }
    }

    function load(filterBar, pageIndex, options) {
        options = options || {};

        if (restoring && !options.restored) {
            return;
        }

        var target = $(filterBar.attr("data-target"));
        var endpoint = filterBar.attr("data-url");

        if (!target.length || !endpoint) {
            return;
        }

        var parameters = readFields(filterBar);

        pageIndex = Number(pageIndex || 0);

        if (!Number.isInteger(pageIndex) || pageIndex < 0) {
            pageIndex = 0;
        }

        // Keep URLs shorter for the first page.
        if (pageIndex > 0) {
            parameters.set("pageIndex", String(pageIndex));
        }

        if (!options.skipHistory) {
            updateAddress(parameters, options.replaceHistory === true);
        }

        var currentVersion = ++requestVersion;

        if (pendingRequest) {
            pendingRequest.abort();
        }

        target.attr("aria-busy", "true");

        var query = parameters.toString();
        var requestUrl = endpoint + (query ? "?" + query : "");

        pendingRequest = $.ajax({
            url: requestUrl,
            type: "GET",
            dataType: "html",
            cache: false
        });

        pendingRequest.done(function (html) {
            if (currentVersion !== requestVersion) {
                return;
            }

            target.html(html);

            if (window.Pager) {
                window.Pager.scan(target[0]);
            }
        });

        pendingRequest.fail(function (xhr, status) {
            if (status === "abort" ||
                currentVersion !== requestVersion) {
                return;
            }

            var response = $("<div>").html(xhr.responseText || "");
            var message = response.find("[data-report-error]").first();

            if (message.length) {
                target.empty().append(message);
            } else {
                target.empty().append(
                    $("<div>")
                        .addClass("alert alert-danger")
                        .attr("role", "alert")
                        .text("بارگذاری گزارش ناموفق بود. دوباره تلاش کنید.")
                );
            }
        });

        pendingRequest.always(function () {
            if (currentVersion === requestVersion) {
                pendingRequest = null;
                target.removeAttr("aria-busy");
            }
        });
    }

    function restoreFromAddress(skipHistory) {
        restoring = true;

        // Invalidate any earlier request before restoring controls.
        requestVersion++;

        if (pendingRequest) {
            pendingRequest.abort();
            pendingRequest = null;
        }

        var parameters =
            new URLSearchParams(window.location.search);

        var values = {};

        parameters.forEach(function (value, key) {
            values[key.toLowerCase()] = value;
        });

        bar.find("input[name], select[name]").each(function () {
            if ($(this).is("[data-fixed-filter]")) {
                return;
            }

            var value = values[this.name.toLowerCase()];

            if (this.type === "checkbox" || this.type === "radio") {
                this.checked = value === this.value;
            } else {
                $(this).val(value === undefined ? "" : value);
            }
        });

        var province = bar.find("[name=ProvinceID]");
        var city = bar.find("[name=CityID]");
        var cityId = values.cityid;

        function finishRestore() {
            refreshLabels(bar);

            restoring = false;

            load(bar, getPageIndex(parameters), {
                restored: true,
                skipHistory: skipHistory,
                replaceHistory: true
            });
        }

        // The Users summary has a dependent city dropdown.
        if (province.length && city.length && province.val()) {
            $.get("/Lookup/Cities", {
                provinceId: province.val()
            })
            .done(function (response) {
                var cities = response && response.data
                    ? response.data
                    : (Array.isArray(response) ? response : []);

                city.html('<option value="">همه شهرها</option>');

                cities.forEach(function (item) {
                    city.append(
                        $("<option>")
                            .val(item.cityID || item.CityID)
                            .text(item.name || item.Name)
                    );
                });

                city.val(cityId || "");
            })
            .fail(function () {
                city.html('<option value="">همه شهرها</option>');

                // Preserve the requested ID if lookup loading fails.
                if (cityId) {
                    city.append(
                        $("<option>")
                            .val(cityId)
                            .text("شهر " + cityId)
                    );

                    city.val(cityId);
                }
            })
            .always(finishRestore);
        } else {
            finishRestore();
        }
    }

    window.ReportSearch = {
        owns: owns,
        load: load
    };

    $(function () {
        restoreFromAddress(false);
    });

    window.addEventListener("popstate", function () {
        var expectedPath =
            root.attr("data-report-page").toLowerCase();

        var currentPath =
            window.location.pathname.toLowerCase();

        if (currentPath !== expectedPath) {
            window.location.reload();
            return;
        }

        restoreFromAddress(true);
    });
})();
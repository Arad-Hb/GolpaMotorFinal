(function () {
    function loadGridPage(wrap, zeroBasedPage) {
        if (!wrap) return;
        var url = wrap.getAttribute("data-list-url");
        var gridId = wrap.getAttribute("data-grid-id");
        if (!url || !gridId) return;

        var page = parseInt(zeroBasedPage, 10);
        if (Number.isNaN(page) || page < 0) return;

        var param = wrap.getAttribute("data-page-param") || "pageIndex";
        var target = document.getElementById(gridId);
        if (!target || typeof window.$ !== "function") return;

        var parsed = new URL(url, window.location.origin);
        parsed.searchParams.set(param, String(page));
        parsed.searchParams.set("_", Date.now().toString());
        window.$(target).load(parsed.pathname + parsed.search, function () {
            if (window.initAdminSelects) window.initAdminSelects(target);
            if (window.Pager && typeof window.Pager.scan === "function") {
                window.Pager.scan(target);
            }
        });
    }

    document.addEventListener("gm-pager:change", function (e) {
        var root = e.target;
        if (!root || !root.classList || !root.classList.contains("gm-pager")) return;
        var wrap = root.closest(".crud-pagination");
        if (!wrap) return;
        var page = (e.detail && e.detail.page) || parseInt(root.getAttribute("data-page"), 10);
        if (!page) return;
        loadGridPage(wrap, page - 1);
    });

    if (window.Pager && typeof window.Pager.scan === "function") {
        if (document.readyState === "loading") {
            document.addEventListener("DOMContentLoaded", function () {
                window.Pager.scan(document);
            });
        } else {
            window.Pager.scan(document);
        }
    }
})();

(function (root, factory) {
    var api = factory();
    if (typeof module === "object" && module.exports) module.exports = api;
    root.Pager = api;
})(typeof window !== "undefined" ? window : this, function () {
    var CHEVRON_LEFT =
        '<svg viewBox="0 0 24 24" aria-hidden="true"><polyline points="15 18 9 12 15 6"></polyline></svg>';
    var CHEVRON_RIGHT =
        '<svg viewBox="0 0 24 24" aria-hidden="true"><polyline points="9 6 15 12 9 18"></polyline></svg>';
    var delegated = false;

    function toInt(value, fallback) {
        var n = parseInt(value, 10);
        return Number.isNaN(n) ? fallback : n;
    }

    function clampPage(page, count) {
        if (count < 1) return 1;
        if (page < 1) return 1;
        if (page > count) return count;
        return page;
    }

    function pageWindow(current, total) {
        if (total <= 5) {
            var all = [];
            for (var i = 1; i <= total; i++) all.push(i);
            return all;
        }

        var set = {};
        var keys = [];
        function add(p) {
            if (p < 1 || p > total || set[p]) return;
            set[p] = true;
            keys.push(p);
        }

        add(1);
        add(current);
        add(total - 1);
        add(total);
        keys.sort(function (a, b) { return a - b; });

        var out = [];
        for (var j = 0; j < keys.length; j++) {
            if (j > 0 && keys[j] - keys[j - 1] > 1) out.push("...");
            out.push(keys[j]);
        }
        return out;
    }

    function readState(el) {
        var count = Math.max(0, toInt(el.getAttribute("data-count"), 0));
        var page = clampPage(toInt(el.getAttribute("data-page"), 1), count || 1);
        return {
            page: page,
            count: count,
            labelGo: el.getAttribute("data-label-go") || "Go to page",
            labelGoBtn: el.getAttribute("data-label-go-btn") || "Go",
            labelPrev: el.getAttribute("data-label-prev") || "Previous",
            labelNext: el.getAttribute("data-label-next") || "Next"
        };
    }

    function emit(el, page) {
        var state = readState(el);
        page = clampPage(page, state.count);
        if (page === state.page) return;
        el.setAttribute("data-page", String(page));
        render(el);
        el.dispatchEvent(new CustomEvent("gm-pager:change", {
            bubbles: true,
            detail: { page: page, pageCount: state.count }
        }));
        if (typeof el._gmOnChange === "function") el._gmOnChange(page, state.count);
    }

    function render(el) {
        if (!el) return;
        var state = readState(el);
        if (state.count <= 1) {
            el.hidden = true;
            el.innerHTML = "";
            return;
        }

        el.hidden = false;
        var prevDisabled = state.page <= 1;
        var nextDisabled = state.page >= state.count;
        var items = pageWindow(state.page, state.count);
        var html = [];

        html.push('<div class="gm-pager__controls">');
        html.push(
            '<button type="button" class="gm-pager__nav gm-pager__nav--prev"' +
            (prevDisabled ? " disabled" : "") +
            ' data-gm-step="-1" aria-label="' + escapeHtml(state.labelPrev) + '">' +
            CHEVRON_LEFT + "</button>"
        );

        html.push('<div class="gm-pager__window" role="list">');
        items.forEach(function (item) {
            if (item === "...") {
                html.push('<span class="gm-pager__ellipsis" aria-hidden="true">...</span>');
                return;
            }
            var current = item === state.page;
            html.push(
                '<button type="button" class="gm-pager__page' + (current ? " is-current" : "") + '"' +
                ' data-gm-page="' + item + '"' +
                (current ? ' aria-current="page"' : "") +
                ">" + item + "</button>"
            );
        });
        html.push("</div>");

        html.push(
            '<button type="button" class="gm-pager__nav gm-pager__nav--next"' +
            (nextDisabled ? " disabled" : "") +
            ' data-gm-step="1" aria-label="' + escapeHtml(state.labelNext) + '">' +
            CHEVRON_RIGHT + "</button>"
        );
        html.push("</div>");

        html.push('<div class="gm-pager__goto">');
        html.push('<div class="gm-pager__jump" data-jump="' + state.page + '">');
        html.push('<span class="gm-pager__label">' + escapeHtml(state.labelGo) + "</span>");
        html.push(
            '<button type="button" class="gm-pager__trigger" aria-expanded="false" aria-haspopup="listbox" aria-label="' +
            escapeHtml(state.labelGo) + '">' +
            '<span class="gm-pager__trigger-label">' + state.page + "</span>" +
            '<span class="gm-pager__caret" aria-hidden="true"></span>' +
            "</button>"
        );
        html.push('<div class="gm-pager__menu" role="listbox">');
        for (var p = 1; p <= state.count; p++) {
            html.push(
                '<button type="button" class="gm-pager__opt' + (p === state.page ? " is-selected" : "") + '"' +
                ' role="option" data-gm-jump="' + p + '"' +
                (p === state.page ? ' aria-selected="true"' : "") +
                ">" + p + "</button>"
            );
        }
        html.push("</div></div>");

        html.push(
            '<button type="button" class="gm-pager__go">' + escapeHtml(state.labelGoBtn) + "</button>"
        );
        html.push("</div>");

        el.innerHTML = html.join("");
    }

    function escapeHtml(value) {
        return String(value)
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;");
    }

    function closeJump(jump) {
        if (!jump) return;
        jump.classList.remove("is-open");
        var trigger = jump.querySelector(".gm-pager__trigger");
        if (trigger) trigger.setAttribute("aria-expanded", "false");
    }

    function closeAllJumps(except) {
        document.querySelectorAll(".gm-pager__jump.is-open").forEach(function (jump) {
            if (jump !== except) closeJump(jump);
        });
    }

    function openJump(jump) {
        if (!jump) return;
        closeAllJumps(jump);
        jump.classList.add("is-open");
        var trigger = jump.querySelector(".gm-pager__trigger");
        if (trigger) trigger.setAttribute("aria-expanded", "true");
        positionMenu(jump);
        var selected = jump.querySelector(".gm-pager__opt.is-selected");
        if (selected && selected.scrollIntoView) {
            selected.scrollIntoView({ inline: "center", block: "nearest" });
        }
    }

    function positionMenu(jump) {
        var menu = jump.querySelector(".gm-pager__menu");
        var trigger = jump.querySelector(".gm-pager__trigger");
        if (!menu || !trigger) return;
        var rect = trigger.getBoundingClientRect();
        var rtl = getComputedStyle(jump).direction === "rtl";
        
        menu.style.position = "fixed";
        menu.style.top = Math.round(rect.bottom + 6) + "px";
        if (rtl) {
            menu.style.right = Math.round(window.innerWidth - rect.right) + "px";
            menu.style.left = "auto";
        } else {
            menu.style.left = Math.round(rect.left) + "px";
            menu.style.right = "auto";
        }
    }

    function onClick(e) {
        var jump = e.target.closest && e.target.closest(".gm-pager__jump");
        if (!jump) closeAllJumps();

        var trigger = e.target.closest && e.target.closest(".gm-pager__trigger");
        if (trigger) {
            e.preventDefault();
            jump = trigger.closest(".gm-pager__jump");
            if (jump.classList.contains("is-open")) closeJump(jump);
            else openJump(jump);
            return;
        }

        var opt = e.target.closest && e.target.closest(".gm-pager__opt");
        if (opt) {
            e.preventDefault();
            jump = opt.closest(".gm-pager__jump");
            jump.querySelectorAll(".gm-pager__opt").forEach(function (btn) {
                btn.classList.toggle("is-selected", btn === opt);
                btn.setAttribute("aria-selected", btn === opt ? "true" : "false");
            });
            jump.setAttribute("data-jump", opt.getAttribute("data-gm-jump"));
            var label = jump.querySelector(".gm-pager__trigger-label");
            if (label) label.textContent = opt.textContent;
            closeJump(jump);
            return;
        }

        var root = e.target.closest && e.target.closest(".gm-pager");
        if (!root) return;

        var nav = e.target.closest(".gm-pager__nav");
        if (nav) {
            if (nav.disabled) return;
            var step = toInt(nav.getAttribute("data-gm-step"), 0);
            emit(root, readState(root).page + step);
            return;
        }

        var pageBtn = e.target.closest(".gm-pager__page");
        if (pageBtn) {
            emit(root, toInt(pageBtn.getAttribute("data-gm-page"), 0));
            return;
        }

        if (e.target.closest(".gm-pager__go")) {
            jump = root.querySelector(".gm-pager__jump");
            var selectedPage = jump ? toInt(jump.getAttribute("data-jump"), 0) : 0;
            emit(root, selectedPage);
        }
    }

    function onKeydown(e) {
        if (e.key !== "Escape") return;
        closeAllJumps();
    }

    function ensureDelegate() {
        if (delegated || typeof document === "undefined") return;
        delegated = true;
        document.addEventListener("click", onClick);
        document.addEventListener("keydown", onKeydown);
    }

    function scan(scope) {
        ensureDelegate();
        var root = scope && scope.querySelectorAll ? scope : document;
        if (!root.querySelectorAll) return;
        root.querySelectorAll(".gm-pager").forEach(function (el) {
            if (!el.firstElementChild) render(el);
        });
    }

    function observe() {
        if (typeof document === "undefined") return;
        ensureDelegate();
        if (typeof MutationObserver === "undefined") {
            if (document.readyState === "loading") {
                document.addEventListener("DOMContentLoaded", function () { scan(document); });
            } else {
                scan(document);
            }
            return;
        }
        var queued = false;
        var observer = new MutationObserver(function () {
            if (queued) return;
            queued = true;
            setTimeout(function () {
                queued = false;
                scan(document);
            }, 0);
        });
        var start = function () {
            if (document.body) observer.observe(document.body, { childList: true, subtree: true });
            scan(document);
        };
        if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", start);
        else start();
    }

    function mount(el, options) {
        if (!el) return;
        options = options || {};
        if (options.page != null) el.setAttribute("data-page", String(options.page));
        if (options.count != null) el.setAttribute("data-count", String(options.count));
        if (options.labelGo) el.setAttribute("data-label-go", options.labelGo);
        if (options.labelGoBtn) el.setAttribute("data-label-go-btn", options.labelGoBtn);
        if (typeof options.onChange === "function") el._gmOnChange = options.onChange;
        ensureDelegate();
        render(el);
        return el;
    }

    observe();

    return { mount: mount, render: render, scan: scan, pageWindow: pageWindow };
});

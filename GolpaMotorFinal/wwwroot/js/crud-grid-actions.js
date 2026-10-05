(() => {
    if (window.crudGridActionsInitialized) {
        return;
    }

    window.crudGridActionsInitialized = true;

    function initGridTooltips(root) {
        if (!window.bootstrap || typeof bootstrap.Tooltip !== "function") {
            return;
        }

        const scope = root instanceof Element ? root : document;
        const tables = scope.matches && scope.matches("table")
            ? [scope]
            : Array.from(scope.querySelectorAll("table"));

        tables.forEach(table => {
            table.querySelectorAll("thead th").forEach(th => {
                const text = (th.getAttribute("data-bs-title") || th.textContent || "")
                    .replace(/\s+/g, " ")
                    .trim();
                if (!text) {
                    return;
                }
                th.setAttribute("data-bs-toggle", "tooltip");
                th.setAttribute("data-bs-title", text);
                th.setAttribute("data-bs-placement", "top");
            });

            table.querySelectorAll(
                ".crud-grid__action-btns a, .crud-grid__action-btns button, .crud-action-option a, .crud-action-option button, .crud-action-toggle"
            ).forEach(btn => {
                const text = (
                    btn.getAttribute("data-bs-title") ||
                    btn.getAttribute("data-title") ||
                    btn.getAttribute("title") ||
                    btn.getAttribute("aria-label") ||
                    ""
                ).replace(/\s+/g, " ").trim();
                if (!text) {
                    return;
                }
                btn.setAttribute("data-bs-toggle", "tooltip");
                btn.setAttribute("data-bs-title", text);
                btn.setAttribute("data-bs-placement", "top");
            });
        });

        scope.querySelectorAll("table [data-bs-toggle='tooltip']").forEach(el => {
            const existing = bootstrap.Tooltip.getInstance(el);
            if (existing) {
                existing.dispose();
            }
            new bootstrap.Tooltip(el, {
                container: "body",
                trigger: "hover focus",
                placement: "top"
            });
        });
    }

    window.initGridTooltips = initGridTooltips;

    function scheduleGridTooltips(root) {
        window.setTimeout(function () {
            initGridTooltips(root || document);
        }, 0);
    }

    if (document.readyState === "loading") {
        document.addEventListener("DOMContentLoaded", function () {
            initGridTooltips(document);
        });
    } else {
        initGridTooltips(document);
    }

    if (window.$) {
        $(document).ajaxComplete(function () {
            scheduleGridTooltips(document);
        });
    }

    const dropdownSelector = ".crud-action-dropdown";

    function setOpen(dropdown, open) {
        const toggle = dropdown.querySelector(".crud-action-toggle");
        const options = dropdown.querySelector(".crud-action-options");

        dropdown.classList.toggle("is-open", open);
        toggle.setAttribute("aria-expanded", String(open));
        options.hidden = !open;
    }

    function closeAll(exceptDropdown = null) {
        document.querySelectorAll(
            `${dropdownSelector}.is-open`
        ).forEach(dropdown => {
            if (dropdown !== exceptDropdown) {
                setOpen(dropdown, false);
            }
        });
    }

    document.addEventListener("click", event => {
        if (!(event.target instanceof Element)) {
            return;
        }

        const toggle = event.target.closest(".crud-action-toggle");

        if (toggle) {
            const dropdown = toggle.closest(dropdownSelector);
            const shouldOpen = !dropdown.classList.contains("is-open");

            closeAll(dropdown);
            setOpen(dropdown, shouldOpen);

            return;
        }

        const actionItem = event.target.closest(".crud-action-option");

        if (actionItem) {
            const dropdown = actionItem.closest(dropdownSelector);

            setOpen(dropdown, false);

            // Do not preventDefault or stopPropagation.
            // Existing navigation/modal/delete handlers still run.
            return;
        }

        if (!event.target.closest(dropdownSelector)) {
            closeAll();
        }
    });

    document.addEventListener("keydown", event => {
        if (event.key !== "Escape") {
            return;
        }

        const focusedDropdown =
            document.activeElement?.closest(dropdownSelector);

        closeAll();

        if (focusedDropdown) {
            focusedDropdown
                .querySelector(".crud-action-toggle")
                .focus();
        }
    });

    document.addEventListener("focusin", event => {
        if (!(event.target instanceof Element)) {
            return;
        }

        closeAll(event.target.closest(dropdownSelector));
    });
})();
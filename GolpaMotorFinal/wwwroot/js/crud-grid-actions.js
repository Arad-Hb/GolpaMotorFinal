(() => {
    // Prevent duplicate event handlers if the script is loaded again.
    if (window.crudGridActionsInitialized) {
        return;
    }

    window.crudGridActionsInitialized = true;

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

    // Delegation also handles rows inserted by an AJAX grid refresh.
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
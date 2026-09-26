(function () {
    function initWarrantyRegisterForm() {
        var container = document.getElementById("warrantyContainer");
        var addBtn = document.getElementById("btnAddWarranty");
        var form = document.getElementById("warrantyRegisterForm");
        var submitBtn = document.getElementById("btnRegisterWarranty");
        if (!container || !addBtn || addBtn.dataset.bound === "1") return;
        addBtn.dataset.bound = "1";

        var roleSelect = document.getElementById("customerTypeSelect");
        var roleRadios = form ? form.querySelectorAll(".warranty-role-buttons input[type='radio']") : [];
        var mobileQuery = window.matchMedia("(max-width: 575.98px)");

        function syncRoleControl() {
            if (!roleSelect || !form) return;
            var mobile = mobileQuery.matches && document.body.classList.contains("pluto-public");
            if (mobile) {
                var checked = form.querySelector(".warranty-role-buttons input[type='radio']:checked");
                if (checked) roleSelect.value = checked.value;
                roleRadios.forEach(function (radio) { radio.disabled = true; });
                roleSelect.disabled = false;
            } else {
                if (roleSelect.value) {
                    roleRadios.forEach(function (radio) {
                        radio.checked = radio.value === roleSelect.value;
                    });
                }
                roleSelect.disabled = true;
                roleRadios.forEach(function (radio) { radio.disabled = false; });
            }
        }

        if (roleSelect) {
            roleSelect.addEventListener("change", function () {
                roleRadios.forEach(function (radio) {
                    radio.checked = radio.value === roleSelect.value;
                });
            });
            if (mobileQuery.addEventListener) mobileQuery.addEventListener("change", syncRoleControl);
            else mobileQuery.addListener(syncRoleControl);
            syncRoleControl();
        }

        var maxCards = parseInt(container.getAttribute("data-max-cards") || "10", 10);
        var lockStorageKey = "warrantyRegisterLockUntil";
        var defaultLabel = "ثبت و فعال‌سازی گارانتی";
        var lockTimer = null;

        function rowCount() {
            return container.querySelectorAll(".warranty-row").length;
        }

        function reindexRows() {
            var rows = container.querySelectorAll(".warranty-row");
            rows.forEach(function (row, i) {
                var input = row.querySelector("input[type='text']");
                if (input) input.name = "ScratchedCode[" + i + "]";
            });
        }

        function syncAddButton() {
            addBtn.disabled = rowCount() >= maxCards;
        }

        addBtn.addEventListener("click", function () {
            if (rowCount() >= maxCards) return;
            var index = rowCount();
            container.insertAdjacentHTML("beforeend",
                '<div class="card mb-3 warranty-row">' +
                    '<div class="card-body">' +
                        '<div class="row g-3 align-items-end warranty-code-row">' +
                            '<div class="col-12 col-md-10 warranty-code-field">' +
                                '<label class="form-label">رمز</label>' +
                                '<input type="text" name="ScratchedCode[' + index + ']" class="form-control" maxlength="50" placeholder="رمز را وارد کنید" />' +
                            '</div>' +
                            '<div class="col-12 col-md-2 warranty-code-remove">' +
                                '<button type="button" class="btn text-danger remove-row"><i class="fa fa-trash"></i></button>' +
                            '</div>' +
                        '</div>' +
                    '</div>' +
                '</div>');
            syncAddButton();
        });

        document.addEventListener("click", function (e) {
            var btn = e.target.closest(".remove-row");
            if (!btn || !container.contains(btn)) return;
            var row = btn.closest(".warranty-row");
            if (row) row.remove();
            reindexRows();
            syncAddButton();
        });

        function remainingLockSeconds() {
            try {
                var until = parseInt(sessionStorage.getItem(lockStorageKey) || "0", 10);
                if (!until) return 0;
                return Math.max(0, Math.ceil((until - Date.now()) / 1000));
            } catch (e) {
                return 0;
            }
        }

        function setLockUntil(seconds) {
            if (!seconds || seconds <= 0) return;
            try {
                sessionStorage.setItem(lockStorageKey, String(Date.now() + seconds * 1000));
            } catch (e) { }
        }

        function applyLockUi() {
            if (!submitBtn) return;
            var remaining = remainingLockSeconds();
            if (lockTimer) {
                clearInterval(lockTimer);
                lockTimer = null;
            }
            if (remaining <= 0) {
                submitBtn.disabled = false;
                submitBtn.textContent = defaultLabel;
                try { sessionStorage.removeItem(lockStorageKey); } catch (e) { }
                return;
            }
            submitBtn.disabled = true;
            submitBtn.textContent = "لطفاً " + remaining + " ثانیه صبر کنید";
            lockTimer = setInterval(applyLockUi, 1000);
        }

        if (form && submitBtn) {
            form.addEventListener("submit", function () {
                if (submitBtn.disabled) return;
                setLockUntil(60);
                applyLockUi();
            });
        }

        var serverRetry = submitBtn ? parseInt(submitBtn.getAttribute("data-retry-after") || "", 10) : 0;
        if (serverRetry > 0) setLockUntil(serverRetry);

        applyLockUi();
        syncAddButton();
    }

    function loadWarrantyGrid() {
        var bar = $("#warranty-cards [data-filter-bar]");
        var grid = $("#warrantyCardsGrid");
        if (!bar.length || !grid.length) return;
        if ($.trim(grid.html()) !== "") return;
        applyTableFilter(bar, 0);
    }

    $(function () {
        initWarrantyRegisterForm();
        if ($("#warranty-cards").hasClass("active"))
            loadWarrantyGrid();
        $("#tab-cards").on("shown.bs.tab", loadWarrantyGrid);
    });
})();

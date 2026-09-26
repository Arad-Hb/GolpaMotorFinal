(function () {
    $(document).on("click", "#toggle-password", function () {
        var passwordInput = document.getElementById("Password");
        var passwordIcon = document.getElementById("toggle-password-icon");
        if (!passwordInput || !passwordIcon) return;
        if (passwordInput.type === "password") {
            passwordInput.type = "text";
            passwordIcon.classList.remove("fa-eye");
            passwordIcon.classList.add("fa-eye-slash");
            this.setAttribute("aria-pressed", "true");
        } else {
            passwordInput.type = "password";
            passwordIcon.classList.remove("fa-eye-slash");
            passwordIcon.classList.add("fa-eye");
            this.setAttribute("aria-pressed", "false");
        }
    });

    $(document).on("change", ".js-user-province", function () {
        var provinceId = $(this).val();
        var city = $(this).closest("form").find(".js-user-city");
        city.empty().append($("<option>").val("").text("انتخاب شهر"));
        if (!provinceId) return;
        $.get("/Lookup/Cities", { provinceId: provinceId }, function (res) {
            if (!res || !res.success || !res.data) return;
            res.data.forEach(function (item) {
                city.append($("<option>").val(item.cityID).text(item.name));
            });
        });
    });
})();

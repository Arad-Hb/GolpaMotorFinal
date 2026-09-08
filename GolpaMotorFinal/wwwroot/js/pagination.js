$(document).on("click", ".crud-pagination a.page-link", function (e) {
    e.preventDefault();
    const $item = $(this).parent();
    if ($item.hasClass("disabled") || $item.hasClass("active")) return;

    const $pager = $(this).closest(".crud-pagination");
    const url = $pager.data("list-url");
    const gridId = $pager.data("grid-id");
    if (!url || !gridId) return;

    const page = parseInt($(this).data("page"), 10);
    if (Number.isNaN(page) || page < 0) return;

    const param = $pager.data("page-param") || "pageIndex";
    const target = $("#" + gridId);
    if (!target.length) return;

    const parsed = new URL(url, window.location.origin);
    parsed.searchParams.set(param, page);
    parsed.searchParams.set("_", Date.now().toString());
    target.load(parsed.pathname + parsed.search);
});

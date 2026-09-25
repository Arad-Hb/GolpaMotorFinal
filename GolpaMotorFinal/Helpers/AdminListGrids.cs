using DomainModel.Models;
using DomainModel.ViewModels.Product;
using DomainModel.ViewModels.Reports;
using DomainModel.ViewModels.Reward;
using DomainModel.ViewModels.Warranty;
using GolpaMotorFinal.Models.ViewModels.CRUD;
using GolpaMotorFinal.Models.ViewModels.UserManagement;

namespace GolpaMotorFinal.Helpers
{
    public static class AdminListGrids
    {
        public static CrudGridViewModel BuildProductGrid(IEnumerable<ProductListItem> items)
        {
            var grid = new CrudGridViewModel { GridId = "ProductGrid", EmptyMessage = "محصولی یافت نشد" };
            grid.Headers.AddRange(new[] { "عکس", "نام محصول", "امتیاز", "وضعیت", "ثبت‌شده", "باقی‌مانده" });

            foreach (var prod in items)
            {
                var id = prod.ProductID.ToString();
                var row = new GridRow { Key = prod.ProductID.ToString() };

                row.Columns.Add(new GridColumn
                {
                    Type = GridColumnType.Image,
                    ImageUrl = ImageHelper.Fix(prod.ImageUrl)
                });
                row.Columns.Add(Text(prod.ProductName));
                row.Columns.Add(Number(prod.ProductPoint));
                row.Columns.Add(Text(prod.IsAvailable ? "موجود" : "ناموجود"));
                row.Columns.Add(Number(prod.RegisteredCardCount));
                row.Columns.Add(Number(prod.UnregisteredCardCount));
                row.Actions.Add(Modal("جزئیات", "fa fa-eye", "/ProductManagement/Details", id, "productID", "btn btn-sm btn-outline-secondary"));
                row.Actions.Add(Modal("ویرایش", "fa fa-pen", "/ProductManagement/Edit", id, "productID", "btn btn-sm btn-outline-warning"));
                row.Actions.Add(new GridAction
                {
                    ActionText = "حذف",
                    OpenModal = false,
                    IsDelete = true,
                    Icon = "fa fa-trash",
                    Url = "/ProductManagement/Delete",
                    Id = id,
                    IdName = "productID",
                    CssClass = "btn btn-sm btn-outline-danger"
                });
                //row.Actions.Add(new GridAction
                //{
                //    ActionText = "حذف عکس",
                //    OpenModal = false,
                //    Icon = "fa fa-image",
                //    Id = id,
                //    IdName = "productID",
                //    CssClass = "btn btn-sm btn-outline-danger btnRemovePicture"
                //});
                grid.Rows.Add(row);
            }

            return grid;
        }

        public static CrudGridViewModel BuildRewardCatalogGrid(IEnumerable<RewardCatalogListItem> items)
        {
            var grid = new CrudGridViewModel { GridId = "CatalogGrid", EmptyMessage = "پاداشی یافت نشد" };
            grid.Headers.AddRange(new[] { "عنوان", "حد نصاب", "نوع", "مبلغ", "وضعیت", "درخواست" });

            foreach (var item in items)
            {
                var id = item.RewardCatalogID.ToString();
                var row = new GridRow { Key = item.RewardCatalogID.ToString() };

                row.Columns.Add(Text(item.Title));
                row.Columns.Add(Number(item.RequiredPoints));
                row.Columns.Add(Text(item.IsCashReward ? "نقدی" : "غیرنقدی"));
                row.Columns.Add(Text(item.IsCashReward ? item.CashValue?.ToString("N0") ?? "-" : "-"));
                row.Columns.Add(Text(item.IsActive ? "فعال" : "غیرفعال"));
                row.Columns.Add(Number(item.RequestCount));
                row.Actions.Add(Modal("جزئیات", "fa fa-eye", "/RewardManagement/Details", id, "rewardCatalogID", "btn btn-sm btn-outline-secondary"));
                row.Actions.Add(Modal("ویرایش", "fa fa-pen", "/RewardManagement/Edit", id, "rewardCatalogID", "btn btn-sm btn-outline-warning"));
                row.Actions.Add(new GridAction
                {
                    ActionText = "غیرفعال",
                    OpenModal = false,
                    IsDelete = true,
                    Icon = "fa fa-trash",
                    Url = "/RewardManagement/Delete",
                    Id = id,
                    IdName = "rewardCatalogID",
                    CssClass = "btn btn-sm btn-outline-danger"
                });
                grid.Rows.Add(row);
            }

            return grid;
        }

        public static CrudGridViewModel BuildRewardRequestGrid(IEnumerable<RewardRequestListItem> items)
        {
            var grid = new CrudGridViewModel { GridId = "RequestGrid", EmptyMessage = "درخواستی یافت نشد" };
            grid.Headers.AddRange(new[] { "کاربر", "موبایل", "پاداش", "حد نصاب", "مانده", "تاریخ", "وضعیت" });

            foreach (var item in items)
            {
                var id = item.RewardRequestID.ToString();
                var pending = !item.IsComplete && item.StatusTitle == RewardStatusTitles.Pending;
                var row = new GridRow { Key = item.RewardRequestID.ToString() };
                var name = string.IsNullOrWhiteSpace(item.UserFullName) ? "نامشخص" : item.UserFullName;
                
                row.Columns.Add(Text(name));
                row.Columns.Add(Text(item.PhoneNumber ?? "-"));
                row.Columns.Add(Text(item.CatalogTitle));
                row.Columns.Add(Number(item.RequiredPoints));
                row.Columns.Add(Number(item.RemainedPoints));
                row.Columns.Add(Text(item.RequestDate.HasValue
                    ? item.RequestDate.Value.ToLocalTime().ToString("yyyy/MM/dd"): "-"));
                row.Columns.Add(Text(item.StatusTitle));
                row.Actions.Add(Modal("جزئیات", "fa fa-eye", "/RewardManagement/RequestDetails", id, "rewardRequestID", "btn btn-sm btn-outline-secondary"));
                row.Actions.Add(new GridAction
                {
                    ActionText = "تأیید",
                    OpenModal = false,
                    Visible = pending,
                    Icon = "fa fa-check",
                    Id = id,
                    IdName = "rewardRequestID",
                    CssClass = "btn btn-sm btn-outline-success btnApproveRequest"
                });
                row.Actions.Add(new GridAction
                {
                    ActionText = "رد",
                    OpenModal = false,
                    Visible = pending,
                    Icon = "fa fa-times",
                    Id = id,
                    IdName = "rewardRequestID",
                    CssClass = "btn btn-sm btn-outline-danger btnRejectRequest"
                });
                grid.Rows.Add(row);
            }

            return grid;
        }

        public static CrudGridViewModel BuildWarrantyReportGrid(IEnumerable<WarrantyProductStatusRow> items)
        {
            var grid = new CrudGridViewModel { GridId = "WarrantyReportGrid", EmptyMessage = "داده‌ای یافت نشد" , ShowRowNumber =true};
            grid.Headers.AddRange(new[] { "محصول", "کل کارت", "ثبت‌شده", "آزاد", "منقضی", "تا ۱۰ روز" });

            foreach (var row in items)
            {
                var gridRow = new GridRow { Key = row.ProductName };

                gridRow.Columns.Add(Text(row.ProductName));
                gridRow.Columns.Add(Number(row.TotalCards));
                gridRow.Columns.Add(Number(row.Registered));
                gridRow.Columns.Add(Number(row.Unregistered));
                gridRow.Columns.Add(Number(row.Expired));
                gridRow.Columns.Add(Number(row.ExpiringSoon));
                grid.Rows.Add(gridRow);
            }

            return grid;
        }

        public static CrudGridViewModel BuildProductReportGrid(IEnumerable<ProductPopularityRow> items)
        {
            var grid = new CrudGridViewModel { GridId = "ProductReportGrid", EmptyMessage = "داده‌ای یافت نشد", ShowRowNumber = true };
            grid.Headers.AddRange(new[] { "محصول", "سال", "ماه", "تعداد ثبت" });

            foreach (var row in items)
            {
                var gridRow = new GridRow { Key = $"{row.ProductName}-{row.JalaliYear}-{row.JalaliMonth}" };

                gridRow.Columns.Add(Text(row.ProductName));
                gridRow.Columns.Add(Number(row.JalaliYear));
                gridRow.Columns.Add(Number(row.JalaliMonth));
                gridRow.Columns.Add(Number(row.Count));
                grid.Rows.Add(gridRow);
            }

            return grid;
        }

        public static CrudGridViewModel BuildRewardReportGrid(IEnumerable<RewardPopularityRow> items)
        {
            var grid = new CrudGridViewModel { GridId = "RewardReportGrid", EmptyMessage = "داده‌ای یافت نشد", ShowRowNumber = true };
            grid.Headers.AddRange(new[] { "پاداش", "کل درخواست", "تأیید", "رد", "در انتظار" });

            foreach (var row in items)
            {
                var gridRow = new GridRow { Key = row.Title };
                gridRow.Columns.Add(Text(row.Title));
                gridRow.Columns.Add(Number(row.RequestCount));
                gridRow.Columns.Add(Number(row.ApprovedCount));
                gridRow.Columns.Add(Number(row.RejectedCount));
                gridRow.Columns.Add(Number(row.PendingCount));
                grid.Rows.Add(gridRow);
            }

            return grid;
        }

        public static CrudGridViewModel BuildUserGrid(IEnumerable<UserListItemViewModel> users)
        {
            var grid = new CrudGridViewModel { GridId = "UserGrid", EmptyMessage = "هیچ کاربری یافت نشد" };
            grid.Headers.AddRange(new[] { "نام", "موبایل", "شغل", "استان", "شهر", "کارت", "امتیاز", "واجد پاداش", "دریافت پاداش" });

            foreach (var item in users)
            {
                var row = new GridRow { Key = item.UserID };
                row.Columns.Add(Text(item.FullName));
                row.Columns.Add(Text(item.PhoneNumber));
                row.Columns.Add(Text(item.RoleName));
                row.Columns.Add(Text(item.Province));
                row.Columns.Add(Text(item.City));
                row.Columns.Add(Number(item.TotalRegisteredCards));
                row.Columns.Add(Number(item.TotalEarnedPoints));
                row.Columns.Add(Text(item.IsEligibleForReward ? "بله" : "خیر"));
                row.Columns.Add(Text(item.HasReceivedReward ? "بله" : "خیر"));

                row.Actions.Add(Modal("ثبت درخواست پاداش", "fa fa-gift", "/UserManagement/EligibleRewards", item.UserID, "userID", "btn btn-sm btn-outline-success"));
                row.Actions.Add(Modal("جزئیات", "fa fa-eye", "/UserManagement/Details", item.UserID, "userID", "btn btn-sm btn-outline-secondary"));
                row.Actions.Add(Modal("ویرایش", "fa fa-pen", "/UserManagement/Edit", item.UserID, "userID", "btn btn-sm btn-outline-warning"));
                row.Actions.Add(new GridAction
                {
                    ActionText = "ادغام حساب",
                    OpenModal = true,
                    Icon = "fa fa-user-plus",
                    Url = "/UserManagement/MergeAccounts",
                    Id = item.UserID,
                    IdName = "userID",
                    CssClass = "btn btn-sm btn-outline-primary",
                    GridId = "UserGrid",
                    RefreshUrl = "/UserManagement/List"
                });
                row.Actions.Add(new GridAction
                {
                    ActionText = "حذف",
                    OpenModal = false,
                    IsDelete = true,
                    Icon = "fa fa-trash",
                    Url = "/UserManagement/Delete",
                    Id = item.UserID,
                    IdName = "userID",
                    CssClass = "btn btn-sm btn-outline-danger",
                    RefreshUrl = "/UserManagement/List",
                    RefreshTargetId = "UserGrid"
                });
                grid.Rows.Add(row);
            }

            return grid;
        }

        public static CrudGridViewModel BuildUserReportGrid(IEnumerable<UserReportViewModel> users)
        {
            var grid = new CrudGridViewModel { GridId = "UserReportGrid", EmptyMessage = "داده‌ای یافت نشد" };
            grid.Headers.AddRange(new[] { "نام", "موبایل", "شغل", "کارت", "امتیاز", "تسویه", "مانده", "استان", "شهر" });

            foreach (var item in users)
            {
                var row = new GridRow { Key = item.UserID };
                row.Columns.Add(Text(item.FullName));
                row.Columns.Add(Text(item.PhoneNumber));
                row.Columns.Add(Text(item.RoleName));
                row.Columns.Add(Number(item.TotalRegisteredCards));
                row.Columns.Add(Number(item.TotalEarnedPoints));
                row.Columns.Add(Number(item.TotalSettledPoints));
                row.Columns.Add(Number(item.RemainedPoints, "fw-bold text-success"));
                row.Columns.Add(Text(item.Province));
                row.Columns.Add(Text(item.City));
                grid.Rows.Add(row);
            }

            return grid;
        }

        public static CrudGridViewModel BuildWarrantyCardGrid(IEnumerable<WarrantyCardListItem> items)
        {
            var grid = new CrudGridViewModel { GridId = "warrantyCardsGrid", EmptyMessage = "کارتی یافت نشد" };
            grid.Headers.AddRange(new[] { "سریال", "رمز", "محصول", "وضعیت", "اعتبار (ماه)", "اعتبار باقی‌مانده" });

            foreach (var card in items)
            {
                var row = new GridRow { Key = card.WarrantyCardID.ToString() };

                row.Columns.Add(Text(card.SerialNumber));
                row.Columns.Add(Text(card.ScratchedCode));
                row.Columns.Add(Text(card.ProductName));
                row.Columns.Add(Text(card.IsRegistered ? "ثبت‌شده" : "ثبت‌‌نشده"));
                row.Columns.Add(Number(card.ValidityMonths));
                row.Columns.Add(Text(card.RemainingText));
                grid.Rows.Add(row);
            }

            return grid;
        }

        private static GridAction Modal(string text, string icon, string url, string id, string idName, string css)
        {
            return new GridAction
            {
                ActionText = text,
                OpenModal = true,
                Icon = icon,
                Url = url,
                Id = id,
                IdName = idName,
                CssClass = css
            };
        }

        private static GridColumn Text(object? value) => new()
        {
            Type = GridColumnType.Text,
            Value = value ?? "-"
        };

        private static GridColumn Number(object? value, string css = "") => new()
        {
            Type = GridColumnType.Number,
            Value = value ?? 0,
            CssClass = css
        };
    }
}

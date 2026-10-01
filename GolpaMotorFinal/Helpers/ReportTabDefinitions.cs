using GolpaMotorFinal.Models.ViewModels.Reports;

namespace GolpaMotorFinal.Helpers
{
    public static class ReportTabDefinitions
    {
        public static IReadOnlyList<ReportTabViewModel> CreateTabs()
        {
            return new List<ReportTabViewModel>
            {
                new ReportTabViewModel { Key = ReportRoutes.Users, Title = "گزارش کاربران" },
                new ReportTabViewModel { Key = ReportRoutes.ProductWarranty, Title = "گزارش گارانتی محصولات" },
                new ReportTabViewModel { Key = ReportRoutes.Rewards, Title = "گزارش پاداش" },
                new ReportTabViewModel { Key = ReportRoutes.UserTransactionDetails, Title = "جزئیات گردش کاربران" },
                new ReportTabViewModel { Key = ReportRoutes.ProductWarrantyTransactionDetails, Title = "جزئیات کارت‌های گارانتی" },
                new ReportTabViewModel { Key = ReportRoutes.RewardTransactionDetails, Title = "جزئیات گردش پاداش" }
            };
        }
    }
}

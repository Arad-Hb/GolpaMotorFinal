using DomainModel.Models;
using DomainModel.ViewModels.Reports;
using System.Linq.Expressions;

namespace DataAccess.Mappers
{
    public static class ReportMapper
    {
        public static Expression<Func<ReportActivityLog, ReportActivityItem>> ToActivityItem =>
            x => new ReportActivityItem
            {
                ReportActivityLogID = x.ReportActivityLogID,
                ActivityType = x.ActivityType,
                OccurredAtUtc = x.OccurredAtUtc,
                UserID = x.UserID,
                UserName = ((x.User.FirstName ?? "") + " " + (x.User.LastName ?? "")).Trim(),
                PhoneNumber = x.User.PhoneNumber,
                ProductID = x.ProductID,
                ProductName = x.Product != null ? x.Product.ProductName : null,
                WarrantyCardID = x.WarrantyCardID,
                SerialNumber = x.WarrantyCard != null ? x.WarrantyCard.SerialNumber : null,
                ScratchedCode = x.WarrantyCard != null ? x.WarrantyCard.ScratchedCode : null,
                RewardRequestID = x.RewardRequestID,
                RewardTitle = x.RewardRequest != null ? x.RewardRequest.RewardCatalog.Title : null,
                StatusTitle = x.StatusTitle,
                PointsDelta = x.PointsDelta,
                TotalEarnedPoints = x.TotalEarnedPoints,
                TotalSettledPoints = x.TotalSettledPoints,
                RemainedPoints = x.RemainedPoints,
                AvailablePoints = x.AvailablePoints,
                Description = x.Description
            };
    }
}

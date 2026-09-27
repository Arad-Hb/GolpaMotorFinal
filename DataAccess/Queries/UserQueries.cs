using DomainModel.Models;
using DomainModel.ViewModels.Reports;
using DomainModel.ViewModels.User;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Queries
{
    public static class UserQueries
    {
        public static IQueryable<ApplicationUser> Active(GolpaMotorDbContext db)
            => db.Users.AsNoTracking().Where(x => !x.IsDeleted);

        public static IQueryable<ApplicationUser> ApplySearch(
            IQueryable<ApplicationUser> query,
            UserSearchModel sm)
        {
            if (!string.IsNullOrEmpty(sm.FirstName))
            {
                query = query.Where(u => u.FirstName.Contains(sm.FirstName));
            }
            if (!string.IsNullOrEmpty(sm.LastName))
            {
                query = query.Where(u => u.LastName.Contains(sm.LastName));
            }
            if (!string.IsNullOrEmpty(sm.SearchTerm))
            {
                query = query.Where(u =>
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(sm.SearchTerm)) ||
                    (u.FirstName != null && u.FirstName.Contains(sm.SearchTerm)) ||
                    (u.LastName != null && u.LastName.Contains(sm.SearchTerm)));
            }
            if (!string.IsNullOrEmpty(sm.PhoneNumber) && string.IsNullOrEmpty(sm.SearchTerm))
            {
                query = query.Where(u => u.PhoneNumber.Contains(sm.PhoneNumber));
            }
            if (!string.IsNullOrEmpty(sm.Email))
            {
                query = query.Where(u => u.Email.Contains(sm.Email));
            }
            if (sm.CustomerTypeID.HasValue && sm.CustomerTypeID.Value > 0)
            {
                query = query.Where(u => u.UserCustomerTypes.Any(t => t.CustomerTypeID == sm.CustomerTypeID.Value));
            }
            if (sm.ProvinceID.HasValue && sm.ProvinceID.Value > 0)
            {
                query = query.Where(u => u.ProvinceID == sm.ProvinceID.Value);
            }
            if (sm.CityID.HasValue && sm.CityID.Value > 0)
            {
                query = query.Where(u => u.CityID == sm.CityID.Value);
            }
            if (sm.PointsFrom.HasValue)
            {
                query = query.Where(u => (u.RemainedPoints ?? 0) >= sm.PointsFrom.Value);
            }
            if (sm.PointsTo.HasValue)
            {
                query = query.Where(u => (u.RemainedPoints ?? 0) <= sm.PointsTo.Value);
            }
            if (sm.IsEligibleForReward.HasValue)
            {
                query = query.Where(u => u.IsEligibleForReward == sm.IsEligibleForReward.Value);
            }
            if (sm.HasReceivedReward.HasValue)
            {
                query = query.Where(u => u.HasReceivedReward == sm.HasReceivedReward.Value);
            }
            if (sm.CardFrom.HasValue || sm.CardTo.HasValue)
            {
                var from = sm.CardFrom ?? DateTime.MinValue;
                var to = sm.CardTo?.Date.AddDays(1) ?? DateTime.MaxValue;
                query = query.Where(u => u.CardRegistrations.Any(c => c.CreatedAt >= from && c.CreatedAt < to));
            }

            return query;
        }

        public static IQueryable<DashboardRegistrarItem> Registrars(GolpaMotorDbContext db)
            => Active(db)
                .Where(x => x.CardRegistrations.Any())
                .Select(x => new DashboardRegistrarItem
                {
                    UserID = x.Id,
                    FullName = ((x.FirstName ?? "") + " " + (x.LastName ?? "")).Trim(),
                    PhoneNumber = x.PhoneNumber,
                    JobTitle = x.UserCustomerTypes
                        .OrderBy(t => t.CustomerTypeID)
                        .Select(t => t.CustomerType.Title)
                        .FirstOrDefault(),
                    Province = x.ProvinceID.HasValue ? x.Province.Name : null,
                    City = x.CityID.HasValue ? x.City.Name : null,
                    IsActive = x.IsActive,
                    RegistrationCount = x.CardRegistrations.Count
                });
    }
}

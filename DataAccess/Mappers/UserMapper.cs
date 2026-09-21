using DomainModel.Models;
using DomainModel.ViewModels.User;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace DataAccess.Mappers
{
    public static class UserMapper
    {
        public static ApplicationUser ToEntity(UserAddEditModel model)
        {
            var user = new ApplicationUser
            {
                UserName = string.IsNullOrWhiteSpace(model.Email)
                            ? $"noemail_{Guid.NewGuid():N}@noemail.local"
                            : model.Email.Trim(),

                Email = string.IsNullOrWhiteSpace(model.Email)
                            ? $"noemail_{Guid.NewGuid():N}@noemail.local"
                            : model.Email.Trim(),

                PhoneNumber = model.PhoneNumber,

                FirstName = model.FirstName?.Trim(),
                LastName = model.LastName?.Trim(),

                ProvinceID = model.ProvinceID,
                CityID = model.CityID,

                Address = string.IsNullOrWhiteSpace(model.Address) ? null : model.Address.Trim(),
                PostalCode = string.IsNullOrWhiteSpace(model.PostalCode) ? null : model.PostalCode.Trim(),

                ProfileImageUrl = string.IsNullOrWhiteSpace(model.ProfileImageUrl)
                    ? null
                    : model.ProfileImageUrl,

                CreditCartNumber = string.IsNullOrWhiteSpace(model.CreditCartNumber)
                    ? null
                    : model.CreditCartNumber,

                IBAN = string.IsNullOrWhiteSpace(model.IBAN)
                    ? null
                    : model.IBAN,

                AccountNumber = string.IsNullOrWhiteSpace(model.AccountNumber)
                    ? null
                    : model.AccountNumber,

                IsActive = model.IsActive,
                IsDeleted = false,
                IsConfirmedCode = false,

                RegisterDate = model.RegisterDate ?? DateTime.Now,

                TotalEarnedPoints = model.TotalEarnedPoints ?? 0,
                TotalSettledPoints = model.TotalSettledPoints ?? 0,
                TotalRegisteredCards = model.TotalRegisteredCards ?? 0
            };

            user.RemainedPoints =
                (user.TotalEarnedPoints ?? 0) -
                (user.TotalSettledPoints ?? 0);

            return user;
        }

        public static void Apply(ApplicationUser user, UserAddEditModel model)
        {
            user.FirstName = model.FirstName?.Trim();
            user.LastName = model.LastName?.Trim();
            user.PhoneNumber = model.PhoneNumber?.Trim();

            user.ProvinceID = model.ProvinceID;
            user.CityID = model.CityID;

            user.Address = model.Address?.Trim();
            user.PostalCode = model.PostalCode?.Trim();

            user.ProfileImageUrl = model.ProfileImageUrl?.Trim();

            user.CreditCartNumber = model.CreditCartNumber?.Trim();
            user.IBAN = model.IBAN?.Trim();
            user.AccountNumber = model.AccountNumber?.Trim();

            user.IsActive = model.IsActive;
        }

        public static UserAddEditModel ToAddEditModel(ApplicationUser model)
        {
            var email = model.Email;
            var emailAttr = new EmailAddressAttribute();
            if (!string.IsNullOrWhiteSpace(email) && !emailAttr.IsValid(email))
            {
                email = null;
            }

            return new UserAddEditModel
            {
                UserID = model.Id,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Email = email,
                PhoneNumber = model.PhoneNumber,

                ProvinceID = model.ProvinceID,
                CityID = model.CityID,
                Address = model.Address,
                PostalCode = model.PostalCode,
                ProfileImageUrl = model.ProfileImageUrl,

                IsActive = model.IsActive,
                IsDeleted = model.IsDeleted,

                CreditCartNumber = model.CreditCartNumber,
                IBAN = model.IBAN,
                AccountNumber = model.AccountNumber,

                CustomerTypeID = model.UserCustomerTypes?.Select(x => (int?)x.CustomerTypeID).FirstOrDefault(),

                TotalEarnedPoints = model.TotalEarnedPoints ?? 0,
                TotalSettledPoints = model.TotalSettledPoints ?? 0,
                RemainedPoints = model.RemainedPoints ?? 0,
                TotalRegisteredCards = model.TotalRegisteredCards ?? 0
            };
        }

        public static Expression<Func<ApplicationUser, UserDetailsModel>> ToDetails =>
            x => new UserDetailsModel
            {
                UserID = x.Id,

                FirstName = x.FirstName ?? string.Empty,
                LastName = x.LastName ?? string.Empty,
                Email = x.Email ?? string.Empty,
                PhoneNumber = x.PhoneNumber ?? string.Empty,

                Province = x.Province != null ? x.Province.Name : string.Empty,
                City = x.City != null ? x.City.Name : string.Empty,
                Address = x.Address ?? string.Empty,
                PostalCode = x.PostalCode ?? string.Empty,

                ProfileImageUrl = x.ProfileImageUrl ?? string.Empty,

                RoleName = x.UserCustomerTypes
                    .Select(uct => uct.CustomerType.Title)
                    .FirstOrDefault() ?? string.Empty,

                IsActive = x.IsActive,
                RegisterDate = x.RegisterDate,

                CreditCartNumber = x.CreditCartNumber ?? string.Empty,
                IBAN = x.IBAN ?? string.Empty,
                AccountNumber = x.AccountNumber ?? string.Empty,

                TotalEarnedPoints = x.TotalEarnedPoints ?? 0,
                TotalSettledPoints = x.TotalSettledPoints ?? 0,
                RemainedPoints = x.RemainedPoints ?? 0,
                TotalRegisteredCards = x.TotalRegisteredCards ?? 0,
                IsEligibleForReward = x.IsEligibleForReward,
                HasReceivedReward = x.HasReceivedReward
            };

        public static Expression<Func<ApplicationUser, UserListItem>> ToListItem =>
            u => new UserListItem
            {
                UserID = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber ?? string.Empty,
                ExistingProfileImageUrl = u.ProfileImageUrl,
                RemainedPoints = u.RemainedPoints ?? 0,
                TotalRegisteredCards = u.TotalRegisteredCards ?? 0,
                TotalEarnedPoints = u.TotalEarnedPoints ?? 0,
                TotalSettledPoints = u.TotalSettledPoints ?? 0,
                Province = u.Province != null ? u.Province.Name : string.Empty,
                City = u.City != null ? u.City.Name : string.Empty,
                RoleName = u.UserCustomerTypes
                    .Select(c => c.CustomerType.Title)
                    .FirstOrDefault() ?? string.Empty,
                IsEligibleForReward = u.IsEligibleForReward,
                HasReceivedReward = u.HasReceivedReward,
            };
    }
}

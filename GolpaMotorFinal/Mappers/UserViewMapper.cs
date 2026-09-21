using DomainModel.Models;
using DomainModel.ViewModels.User;
using GolpaMotorFinal.Models.ViewModels.CRUD;
using GolpaMotorFinal.Models.ViewModels.UserManagement;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace GolpaMotorFinal.Mappers
{
    public static class UserViewMapper
    {
        public static UserAddEditModel ToAddEditModel(UserAddEditViewModel vm)
        {
            return new UserAddEditModel
            {
                UserID = vm.UserID,
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                Email = vm.Email,
                PhoneNumber = vm.PhoneNumber,
                CustomerTypeID = vm.CustomerTypeID,
                ProvinceID = vm.ProvinceID,
                CityID = vm.CityID,
                Address = vm.Address,
                PostalCode = vm.PostalCode,
                CreditCartNumber = vm.CreditCartNumber,
                IBAN = vm.IBAN,
                AccountNumber = vm.AccountNumber,
                IsActive = vm.IsActive,
                IsDeleted = vm.IsDeleted,
                ProfileImageUrl = vm.ProfileImageUrl
            };
        }

        public static UserAddEditViewModel ToAddEditViewModel(
            UserAddEditModel user,
            IEnumerable<CustomerType> customerTypes,
            IEnumerable<Province> provinces,
            IEnumerable<City> cities,
            CrudFormViewModel form)
        {
            user ??= new UserAddEditModel();
            return new UserAddEditViewModel
            {
                UserID = user.UserID,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                CustomerTypeID = user.CustomerTypeID,
                ProvinceID = user.ProvinceID,
                CityID = user.CityID,
                Address = user.Address,
                PostalCode = user.PostalCode,
                CreditCartNumber = user.CreditCartNumber,
                IBAN = user.IBAN,
                AccountNumber = user.AccountNumber,
                IsActive = user.IsActive,
                IsDeleted = user.IsDeleted,
                ProfileImageUrl = user.ProfileImageUrl,
                CustomerTypes = new SelectList(customerTypes, "CustomerTypeID", "Title", user.CustomerTypeID),
                Provinces = new SelectList(provinces, "ProvinceID", "Name", user.ProvinceID),
                Cities = new SelectList(cities, "CityID", "Name", user.CityID),
                CrudFormViewModel = form
            };
        }

        public static UserListItemViewModel ToListItem(UserListItem u)
        {
            return new UserListItemViewModel
            {
                UserID = u.UserID,
                FullName = $"{u.FirstName ?? string.Empty} {u.LastName ?? string.Empty}".Trim(),
                PhoneNumber = u.PhoneNumber ?? string.Empty,
                ProfileImageUrl = u.ExistingProfileImageUrl ?? string.Empty,
                RoleName = u.RoleName ?? string.Empty,
                TotalRegisteredCards = u.TotalRegisteredCards,
                TotalEarnedPoints = u.TotalEarnedPoints,
                IsEligibleForReward = u.IsEligibleForReward,
                HasReceivedReward = u.HasReceivedReward,
                Province = u.Province ?? string.Empty,
                City = u.City ?? string.Empty
            };
        }

        public static UserReportViewModel ToReport(UserListItem u)
        {
            return new UserReportViewModel
            {
                UserID = u.UserID,
                FullName = $"{u.FirstName ?? string.Empty} {u.LastName ?? string.Empty}".Trim(),
                PhoneNumber = u.PhoneNumber ?? string.Empty,
                RoleName = u.RoleName ?? string.Empty,
                TotalRegisteredCards = u.TotalRegisteredCards,
                TotalEarnedPoints = u.TotalEarnedPoints,
                TotalSettledPoints = u.TotalSettledPoints,
                RemainedPoints = u.RemainedPoints,
                ProfileImageUrl = u.ExistingProfileImageUrl ?? string.Empty,
                Province = u.Province ?? string.Empty,
                City = u.City ?? string.Empty
            };
        }

        public static MergeAccountsViewModel ToMerge(UserDetailsModel user)
        {
            if (user == null)
                return new MergeAccountsViewModel();

            return new MergeAccountsViewModel
            {
                UserID = user.UserID,
                FullName = $"{user.FirstName} {user.LastName}".Trim(),
                PhoneNumber = user.PhoneNumber,
                TotalEarnedPoints = user.TotalEarnedPoints,
                TotalSettledPoints = user.TotalSettledPoints,
                RemainedPoints = user.RemainedPoints,
                TotalRegisteredCards = user.TotalRegisteredCards,
                ProfileImageUrl = user.ProfileImageUrl
            };
        }
    }
}

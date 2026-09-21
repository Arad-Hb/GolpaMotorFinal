using DomainModel.Models;
using DomainModel.ViewModels.Warranty;

namespace DataAccess.Mappers
{
    public static class WarrantyCardMapper
    {
        public static WarrantyCard ToEntity(long productId, string serialNumber, string scratchedCode, int validityMonths = 12)
        {
            return new WarrantyCard
            {
                ProductID = productId,
                SerialNumber = serialNumber,
                ScratchedCode = scratchedCode,
                IsRegistered = false,
                ValidityMonths = validityMonths
            };
        }

        public static WarrantyCard ToEntity(WarrantyCardImportItem item, long productId, int defaultValidityMonths = 12)
            => ToEntity(productId, item.SerialNumber, item.ScratchedCode, item.ValidityMonths ?? defaultValidityMonths);
    }
}

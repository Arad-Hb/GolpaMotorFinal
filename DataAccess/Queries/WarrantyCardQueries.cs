using DomainModel.Models;
using Microsoft.EntityFrameworkCore;

namespace DataAccess.Queries
{
    public static class WarrantyCardQueries
    {
        public static IQueryable<WarrantyCard> All(GolpaMotorDbContext db)
            => db.WarrantyCards.AsNoTracking();
    }
}

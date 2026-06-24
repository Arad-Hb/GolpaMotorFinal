using DataAccess.Services;
using DomainModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Repositories
{
    public class WarrantyCardRepository : IWarrantyCardRepository
    {
        private readonly GolpaMotorDbContext db;

        public WarrantyCardRepository(GolpaMotorDbContext db)
        {
            this.db = db;
        }

        public void AddRange(List<WarrantyCard> cards)
        {
            db.WarrantyCards.AddRange(cards);
        }

        public void Save()
        {
            db.SaveChanges();
        }
    }
}

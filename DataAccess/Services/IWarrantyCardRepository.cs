using DomainModel.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Services
{
    public interface IWarrantyCardRepository
    {
        void AddRange(List<WarrantyCard> cards);
        void Save();        
    }
}

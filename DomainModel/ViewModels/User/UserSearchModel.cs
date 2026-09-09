using Framework.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.ViewModels.User
{
    public class UserSearchModel : PageModel
    {
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }

        public string? SearchTerm { get; set; }

        public bool? IsActive { get; set; }

        public int? CustomerTypeID { get; set; }

        public int? ProvinceID { get; set; }

        public int? CityID { get; set; }

        public int? PointsFrom { get; set; }

        public int? PointsTo { get; set; }

        public bool? IsEligibleForReward { get; set; }

        public bool? HasReceivedReward { get; set; }

        public DateTime? CardFrom { get; set; }

        public DateTime? CardTo { get; set; }

        public string? CardFromJalali { get; set; }

        public string? CardToJalali { get; set; }

    }
}

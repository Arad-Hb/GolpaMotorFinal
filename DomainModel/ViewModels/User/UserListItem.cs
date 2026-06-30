using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.ViewModels.User
{
    public class UserListItem
    {
        public string UserID { get; set; }
        public string? FirstName { get; set; }

        public string? LastName { get; set; }

        public string? Province { get; set; }

        public string? City { get; set; }
        public string? Email { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string? ExistingProfileImageUrl { get; set; }
        public DateTime RegisterDate { get; set; }
        public int? RemainedPoints { get; set; }

    }
}
   


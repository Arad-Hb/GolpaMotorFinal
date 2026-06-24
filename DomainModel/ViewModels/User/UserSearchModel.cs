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

        public bool? IsActive { get; set; }
        
    }
}

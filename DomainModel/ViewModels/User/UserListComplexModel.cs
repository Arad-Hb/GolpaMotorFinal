using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DomainModel.ViewModels.User
{
    public class UserListComplexModel
    {
        public List<UserListItem>? userList {get; set;}
        public UserSearchModel? sm {get; set;}
        public UserListComplexModel()
        {
            userList = new List<UserListItem>();
        }
    }
}

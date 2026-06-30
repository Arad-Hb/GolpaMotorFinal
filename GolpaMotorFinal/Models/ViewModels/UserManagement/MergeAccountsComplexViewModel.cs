namespace GolpaMotorFinal.Models.ViewModels.UserManagement
{
    public class MergeAccountsComplexViewModel
    {
        public MergeAccountsViewModel CurrentUser { get; set; } 

        public MergeAccountsViewModel? SearchedUser { get; set; }
        public SearchBoxViewModel Search { get; set; } = new();
    }

}


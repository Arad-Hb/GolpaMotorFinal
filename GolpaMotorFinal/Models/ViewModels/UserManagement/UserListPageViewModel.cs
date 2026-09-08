namespace GolpaMotorFinal.Models.ViewModels.UserManagement
{
    public class UserListPageViewModel
    {
        public List<UserListItemViewModel> Items { get; set; } = new();
        public int PageIndex { get; set; }
        public int PageCount { get; set; }
        public int RecordCount { get; set; }
        public string? SearchTerm { get; set; }
    }
}

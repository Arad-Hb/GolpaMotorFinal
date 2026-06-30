namespace GolpaMotorFinal.Models.ViewModels.UserManagement
{
    public class MergeAccountsViewModel
    {
        public string UserID { get; set; }
        public string? SelectedMergeUserID { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string? ProfileImageUrl { get; set; }
        public int RemainedPoints { get; set; }
    }
}

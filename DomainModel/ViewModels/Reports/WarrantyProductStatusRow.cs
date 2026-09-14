namespace DomainModel.ViewModels.Reports
{
    public class WarrantyProductStatusRow
    {
        public string ProductName { get; set; } = string.Empty;
        public int TotalCards { get; set; }
        public int Registered { get; set; }
        public int Unregistered { get; set; }
        public int Expired { get; set; }
        public int ExpiringSoon { get; set; }
    }
}

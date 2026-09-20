namespace GolpaMotorFinal.Models.ViewModels
{
    public class FilterSearchViewModel
    {
        public string Name { get; set; } = "SearchTerm";
        public string Placeholder { get; set; } = "جستجو...";
        public string? Value { get; set; }

        public static FilterSearchViewModel For(string name, string placeholder, string? value = null)
            => new() { Name = name, Placeholder = placeholder, Value = value };
    }

    public class NumberRangeFilterViewModel
    {
        public string FromName { get; set; } = "From";
        public string ToName { get; set; } = "To";
        public string EmptyLabel { get; set; } = "بازه عدد";
        public string FromPlaceholder { get; set; } = "حداقل";
        public string ToPlaceholder { get; set; } = "حداکثر";
        public string Step { get; set; } = "1";
        public string? FromValue { get; set; }
        public string? ToValue { get; set; }

        public static NumberRangeFilterViewModel For(
            string fromName,
            string toName,
            string label,
            string fromPlaceholder,
            string toPlaceholder,
            string? fromValue = null,
            string? toValue = null)
            => new()
            {
                FromName = fromName,
                ToName = toName,
                EmptyLabel = label,
                FromPlaceholder = fromPlaceholder,
                ToPlaceholder = toPlaceholder,
                FromValue = fromValue,
                ToValue = toValue
            };
    }
}

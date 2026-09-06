namespace GolpaMotorFinal.Models.ViewModels.CRUD
{
    public class CrudFormViewModel
    {
        public string FormId { get; set; } = "crudForm";

        public string Method { get; set; } = "post";

        public string Title { get; set; } = string.Empty;

        public string Controller { get; set; } = string.Empty;

        public string Action { get; set; } = string.Empty;

        public bool Ajax { get; set; } = true;

        public string SubmitButtonText { get; set; } = "ذخیره";

        public bool RefreshGrid { get; set; } = true;

        public string? GridId { get; set; }

        public string? RefreshGridUrl { get; set; }

        public Dictionary<string, object?> RefreshGridRouteValues { get; set; } = new();

        public Dictionary<string, object?> RouteValues { get; set; } = new();

        public Dictionary<string, string> HiddenFields { get; set; } = new();

        public Dictionary<string, string> HtmlAttributes { get; set; } = new();

        public string ModalSize { get; set; } = "lg";

        public string Enctype { get; set; } = "multipart/form-data";

        public bool HasFileUpload { get; set; }

        public bool CloseOnSuccess { get; set; } = true;
    }
}

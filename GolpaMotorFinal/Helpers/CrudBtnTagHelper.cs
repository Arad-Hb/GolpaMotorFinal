using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GolpaMotorFinal.Helpers
{
    [HtmlTargetElement("crud-btn")]
    public class CrudBtnTagHelper : TagHelper
    {
        public string Type { get; set; } = "custom";
        public string Url { get; set; } = "#";
        public string Title { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public string CssClass { get; set; } = "btn btn-sm";
        public string GridId { get; set; } = string.Empty;
        public string IdParam { get; set; } = "id";
        public string ConfirmMessage { get; set; } = "آیا مطمئن هستید؟";
        public string Id { get; set; } = string.Empty;

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "button";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.SetAttribute("type", "button");

            var css = CssClass;
            var iconHtml = string.IsNullOrEmpty(Icon) ? "" : $"<i class=\"fa {Icon}\"></i> ";
            var text = string.IsNullOrEmpty(Text) ? "" : Text;

            switch (Type.ToLower())
            {
                case "create":
                    output.Attributes.SetAttribute("class", $"{css} btn-success open-modal");
                    output.Attributes.SetAttribute("data-url", Url);
                    output.Attributes.SetAttribute("data-title", Title);
                    output.Content.SetHtmlContent($"{iconHtml}{text}");
                    break;

                case "edit":
                    output.Attributes.SetAttribute("class", $"{css} btn-warning open-modal btn-crud-edit");
                    output.Attributes.SetAttribute("data-url", Url);
                    output.Attributes.SetAttribute("data-title", Title);
                    output.Attributes.SetAttribute("data-id-param", IdParam);
                    if (!string.IsNullOrEmpty(Id))
                        output.Attributes.SetAttribute("data-id", Id);
                    output.Content.SetHtmlContent($"{iconHtml}{text}");
                    break;

                case "details":
                    output.Attributes.SetAttribute("class", $"{css} btn-info open-modal btn-crud-details");
                    output.Attributes.SetAttribute("data-url", Url);
                    output.Attributes.SetAttribute("data-title", Title);
                    output.Attributes.SetAttribute("data-id-param", IdParam);
                    if (!string.IsNullOrEmpty(Id))
                        output.Attributes.SetAttribute("data-id", Id);
                    output.Content.SetHtmlContent($"{iconHtml}{text}");
                    break;

                case "delete":
                    output.Attributes.SetAttribute("class", $"{css} btn-danger btn-crud-delete");
                    output.Attributes.SetAttribute("data-url", Url);
                    output.Attributes.SetAttribute("data-id-param", IdParam);
                    output.Attributes.SetAttribute("data-confirm", ConfirmMessage);
                    if (!string.IsNullOrEmpty(GridId))
                        output.Attributes.SetAttribute("data-grid-id", GridId);
                    if (!string.IsNullOrEmpty(Id))
                        output.Attributes.SetAttribute("data-id", Id);
                    output.Content.SetHtmlContent($"{iconHtml}{text}");
                    break;

                default:
                    output.Attributes.SetAttribute("class", css);
                    output.Content.SetHtmlContent($"{iconHtml}{text}");
                    break;
            }
        }
    }
}

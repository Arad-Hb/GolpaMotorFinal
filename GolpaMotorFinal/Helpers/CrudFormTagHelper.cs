using GolpaMotorFinal.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;

namespace GolpaMotorFinal.Helpers
{
    [HtmlTargetElement("crud-form")]
    public class CrudFormTagHelper : TagHelper
    {
        private readonly IUrlHelperFactory _urlFactory;

        public CrudFormTagHelper(IUrlHelperFactory urlFactory)
        {
            _urlFactory = urlFactory;
        }

        [HtmlAttributeName("model")]
        public CrudFormViewModel Model { get; set; } = default!;

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; } = default!;

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "form";
            output.TagMode = TagMode.StartTagAndEndTag;

            var urlHelper = _urlFactory.GetUrlHelper(ViewContext);
            var actionUrl = urlHelper.Action(Model.Action, Model.Controller, Model.RouteValues) ?? "#";

            output.Attributes.SetAttribute("id", Model.FormId);
            output.Attributes.SetAttribute("method", Model.Method.ToLower());
            output.Attributes.SetAttribute("action", actionUrl);
            output.Attributes.SetAttribute("class", "crud-form");
            output.Attributes.SetAttribute("enctype", Model.Enctype);

            if (Model.Ajax)
                output.Attributes.SetAttribute("data-ajax", "true");

            if (!string.IsNullOrEmpty(Model.GridId))
                output.Attributes.SetAttribute("data-grid-id", Model.GridId);

            if (!string.IsNullOrEmpty(Model.RefreshGridUrl))
            {
                output.Attributes.SetAttribute(
                    "data-refresh-grid-url",
                    urlHelper.Action(
                        Model.RefreshGridUrl,
                        Model.Controller,
                        Model.RefreshGridRouteValues));
            };

            output.Attributes.SetAttribute("data-refresh-grid", Model.RefreshGrid.ToString().ToLower());
            output.Attributes.SetAttribute("data-close-on-success", Model.CloseOnSuccess.ToString().ToLower());

            foreach (var attr in Model.HtmlAttributes)
                output.Attributes.SetAttribute(attr.Key, attr.Value);

            var child = await output.GetChildContentAsync();
            var hiddenFields = new StringBuilder();

            foreach (var field in Model.HiddenFields)
                hiddenFields.AppendLine($"<input type=\"hidden\" name=\"{field.Key}\" value=\"{field.Value}\" />");

            output.Content.SetHtmlContent(hiddenFields.ToString() + child.GetContent());
        }
    }
}

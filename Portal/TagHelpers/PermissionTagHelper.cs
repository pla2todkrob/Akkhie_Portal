using Microsoft.AspNetCore.Razor.TagHelpers;
using Portal.Interfaces;

namespace Portal.TagHelpers
{
    [HtmlTargetElement(Attributes = "asp-permission")]
    public class PermissionTagHelper(IViewPermissionService permissionService) : TagHelper
    {
        public string AspPermission { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            if (!await permissionService.HasPermissionAsync(AspPermission))
            {
                output.SuppressOutput();
            }
        }
    }
}
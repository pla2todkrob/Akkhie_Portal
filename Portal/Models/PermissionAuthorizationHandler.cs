using Microsoft.AspNetCore.Authorization;
using Portal.Interfaces;
using System.Threading.Tasks;
public class PermissionAuthorizationHandler(IViewPermissionService permissionService)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        if (await permissionService.HasPermissionAsync(requirement.Permission))
        {
            context.Succeed(requirement);
        }
    }
}
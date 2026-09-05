using Microsoft.AspNetCore.Mvc.Filters;
using OpenToWork.Shared.Enums;

namespace OpenToWork.AdminAPI.Authorization;

/// <summary>
/// Restringe un controller/action a uno o mas AdminStaffRole especificos. SuperAdmin siempre
/// pasa, sin importar la lista. Se apila sobre el [Authorize(Roles = "Admin")] que ya aplica
/// AdminControllerBase — esto solo agrega una segunda verificacion sobre el sub-rol de staff.
/// Sin argumentos (lista vacia) equivale a "solo SuperAdmin".
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RequireStaffRoleAttribute : Attribute, IAuthorizationFilter
{
    private readonly AdminStaffRole[] _allowed;

    public RequireStaffRoleAttribute(params AdminStaffRole[] allowed)
    {
        _allowed = allowed;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var claim = context.HttpContext.User.FindFirst("staffRole")?.Value;
        if (!int.TryParse(claim, out var roleValue))
        {
            context.Result = new Microsoft.AspNetCore.Mvc.ForbidResult();
            return;
        }

        var role = (AdminStaffRole)roleValue;
        if (role == AdminStaffRole.SuperAdmin || _allowed.Contains(role))
            return;

        context.Result = new Microsoft.AspNetCore.Mvc.ForbidResult();
    }
}

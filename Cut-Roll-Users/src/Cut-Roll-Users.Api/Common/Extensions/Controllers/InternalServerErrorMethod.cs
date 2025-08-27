
namespace Cut_Roll_Users.Api.Common.Extensions.Controllers;

using Microsoft.AspNetCore.Mvc;

public static class InternalServerErrorMethod
{
    public static IActionResult InternalServerError(this ControllerBase controller, string message)
    {
        return controller.StatusCode(500, new { message });
    }
}
using LibraryManagement.AppServices.Common;
using LibraryManagement.AppServices.DTOs.Common;
using Microsoft.AspNetCore.Mvc;

namespace LibraryManagement.API.Controllers;

[ApiController]
[Produces("application/json")]
public abstract class BaseApiController : ControllerBase
{
    protected IActionResult HandleResult<T>(ServiceResult<T> result)
        => result.Succeeded
            ? Ok(ApiResponse<T>.Ok(result.Data!, result.Message))
            : ToError(result);

    protected IActionResult HandleResult(ServiceResult result)
        => result.Succeeded
            ? Ok(ApiResponse.Ok(result.Message))
            : ToError(result);

    private IActionResult ToError(ServiceResult result)
    {
        var body = ApiResponse.Fail(result.Message);

        return result.Error switch
        {
            ServiceError.NotFound => NotFound(body),
            ServiceError.Conflict => Conflict(body),
            ServiceError.Unauthorized => Unauthorized(body),
            _ => BadRequest(body)
        };
    }
}

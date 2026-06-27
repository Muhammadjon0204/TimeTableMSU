using Application.Common;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult ToOkResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return BadRequest(new { error = result.Error });
    }

    protected ActionResult ToCreatedResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return StatusCode(StatusCodes.Status201Created, result.Value);
        }

        return BadRequest(new { error = result.Error });
    }

    protected ActionResult ToCreatedIdResult(Result<int> result)
    {
        if (result.IsSuccess)
        {
            return StatusCode(StatusCodes.Status201Created, new { id = result.Value });
        }

        return BadRequest(new { error = result.Error });
    }

    protected ActionResult ToNoContentResult(Result result)
    {
        if (result.IsSuccess)
        {
            return NoContent();
        }

        return BadRequest(new { error = result.Error });
    }

    protected ActionResult IdMismatch()
    {
        return BadRequest(new { error = "Route id does not match request body id" });
    }
}

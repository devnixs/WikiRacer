using Microsoft.AspNetCore.Mvc;
using WikiRacer.Application.Lobbies;
using WikiRacer.Contracts.Errors;

namespace WikiRacer.Api.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected ActionResult LobbyError(LobbyOperationException exception)
    {
        return StatusCode(
            exception.StatusCode,
            new ErrorPayload(exception.ErrorCode, exception.Message));
    }

    protected BadRequestObjectResult ValidationError(ArgumentException exception)
    {
        return BadRequest(new ErrorPayload("validation_failed", exception.Message));
    }
}

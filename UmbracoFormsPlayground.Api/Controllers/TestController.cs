using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace UmbracoFormsPlayground.Api.Controllers;

/// <summary>
/// Simple endpoint for checking that the API is wired into the site and responding.
/// </summary>
/// <remarks>
/// Deliberately public (<see cref="AllowAnonymousAttribute"/>): it returns only a fixed status
/// and the server time, nothing sensitive. Any endpoint that returns real data must require
/// authorisation instead.
/// </remarks>
[ApiController]
[Route("api/test")]
[AllowAnonymous]
public sealed class TestController : ControllerBase
{
    /// <summary>Returns a fixed status and the current UTC server time.</summary>
    /// <response code="200">The API is responding.</response>
    [HttpGet("ping")]
    [ProducesResponseType<PingResponse>(StatusCodes.Status200OK)]
    public ActionResult<PingResponse> Ping() => Ok(new PingResponse("ok", DateTimeOffset.UtcNow));
}

/// <summary>Response body for <see cref="TestController.Ping"/>.</summary>
/// <param name="Status">Always <c>ok</c> when the API responds.</param>
/// <param name="ServerTimeUtc">Server time when the request was handled.</param>
public sealed record PingResponse(string Status, DateTimeOffset ServerTimeUtc);

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core.DeliveryApi;
using Umbraco.Cms.Core.Models.DeliveryApi;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services;

namespace UmbracoFormsPlayground.Api.Controllers;

/// <summary>
/// Returns published Umbraco content by its integer node id or its GUID key.
/// </summary>
/// <remarks>
/// Public by design, but only ever returns what the public website already shows:
/// <list type="bullet">
/// <item>Content is read from the published cache with preview off, so drafts and unpublished
/// nodes are never returned.</item>
/// <item>Nodes under Public Access (member login) protection return 404, the same as a missing
/// node, so the endpoint cannot be used to bypass member login or to discover protected ids.</item>
/// <item>The response is built with Umbraco's Delivery API builder, so property values are
/// converted to safe JSON rather than serialising raw .NET objects.</item>
/// </list>
/// </remarks>
[ApiController]
[Route("api/content")]
[AllowAnonymous]
public sealed class ContentController : ControllerBase
{
    private readonly IPublishedContentCache _contentCache;
    private readonly IPublicAccessService _publicAccessService;
    private readonly IApiContentResponseBuilder _responseBuilder;

    /// <summary>Creates the controller.</summary>
    /// <param name="contentCache">Published content cache.</param>
    /// <param name="publicAccessService">Used to exclude member-protected content.</param>
    /// <param name="responseBuilder">Builds the Delivery API shaped response.</param>
    public ContentController(
        IPublishedContentCache contentCache,
        IPublicAccessService publicAccessService,
        IApiContentResponseBuilder responseBuilder)
    {
        _contentCache = contentCache;
        _publicAccessService = publicAccessService;
        _responseBuilder = responseBuilder;
    }

    /// <summary>Gets a published content node by its integer id.</summary>
    /// <param name="id">The node id, as shown in the backoffice Info tab.</param>
    /// <response code="200">The published content node.</response>
    /// <response code="404">No published, publicly accessible node has this id.</response>
    [HttpGet("{id:int}")]
    [ProducesResponseType<IApiContentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(int id)
        => ToResult(await _contentCache.GetByIdAsync(id, preview: false));

    /// <summary>Gets a published content node by its GUID key.</summary>
    /// <param name="key">The node key (GUID), as shown in the backoffice Info tab and returned as <c>id</c> by this API.</param>
    /// <response code="200">The published content node.</response>
    /// <response code="404">No published, publicly accessible node has this key.</response>
    [HttpGet("{key:guid}")]
    [ProducesResponseType<IApiContentResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByKey(Guid key)
        => ToResult(await _contentCache.GetByIdAsync(key, preview: false));

    /// <summary>
    /// Applies the same visibility rules to every lookup: missing, member-protected and
    /// unroutable content all return 404 so they cannot be told apart.
    /// </summary>
    private IActionResult ToResult(IPublishedContent? content)
    {
        if (content is null || _publicAccessService.IsProtected(content.Path).Success)
        {
            return NotFound();
        }

        var response = _responseBuilder.Build(content);
        return response is null ? NotFound() : Ok(response);
    }
}

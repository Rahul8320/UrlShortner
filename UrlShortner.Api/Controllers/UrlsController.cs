using Microsoft.AspNetCore.Mvc;
using UrlShortner.Api.Services.Interface;

namespace UrlShortner.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
internal class UrlsController(IUrlShortnerService urlShortnerService, ILogger<UrlsController> logger) : ControllerBase
{
    [HttpPost]
    [Route("shorten")]
    public async Task<IResult> Shorten([FromBody] string url)
    {
        try
        {
            if (Uri.TryCreate(url, UriKind.Absolute, out _) == false)
            {
                return Results.BadRequest("Invalid URL format");
            }

            var shortcode = await urlShortnerService.ShortenUrl(url);

            return Results.Ok(new { shortcode });
        }
        catch (Exception ex)
        {
            logger.LogError(message: ex.Message, args: ex);
            return Results.InternalServerError(ex.Message);
        }
    }

    [HttpGet]
    [Route("{shortcode:string}")]
    public async Task<IResult> Get([FromRoute] string shortcode)
    {
        try
        {
            var originalUrl = await urlShortnerService.GetOriginalUrl(shortcode);

            return originalUrl is null ? Results.NotFound() : Results.Redirect(originalUrl);
        }
        catch (Exception ex)
        {
            logger.LogError(message: ex.Message, args: ex);
            return Results.InternalServerError(ex.Message);
        }
    }

    [HttpGet]
    public async Task<IResult> GetAll()
    {
        try
        {
            var allUrls = await urlShortnerService.GetAllUrls();

            return Results.Ok(allUrls);
        }
        catch (Exception ex)
        {
            logger.LogError(message: ex.Message, args: ex);
            return Results.InternalServerError(ex.Message);
        }
    }
}

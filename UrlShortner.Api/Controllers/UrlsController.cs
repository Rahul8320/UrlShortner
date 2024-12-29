using Microsoft.AspNetCore.Mvc;
using UrlShortner.Api.Model;
using UrlShortner.Api.Services.Interface;

namespace UrlShortner.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UrlsController(IUrlShortnerService urlShortnerService, ILogger<UrlsController> logger) : ControllerBase
{
    [HttpPost]
    [Route("shorten")]
    public async Task<IResult> Shorten([FromBody] UrlRequest request)
    {
        try
        {
            if (Uri.TryCreate(request.Url, UriKind.Absolute, out _) == false)
            {
                return Results.BadRequest("Invalid URL format");
            }

            var shortcode = await urlShortnerService.ShortenUrl(request.Url);

            return Results.Ok(new { shortcode });
        }
        catch (Exception ex)
        {
            logger.LogError(message: ex.Message, args: ex);
            return Results.InternalServerError(ex.Message);
        }
    }

    [HttpGet]
    [Route("{shortcode}")]
    public async Task<IResult> Get([FromRoute] string shortcode)
    {
        try
        {
            var originalUrl = await urlShortnerService.GetOriginalUrl(shortcode);

            return originalUrl is null ? Results.NotFound() : Results.Redirect(originalUrl);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, ex.Message);
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
            logger.LogError(ex, ex.Message);
            return Results.InternalServerError(ex.Message);
        }
    }
}

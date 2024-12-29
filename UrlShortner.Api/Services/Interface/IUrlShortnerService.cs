using UrlShortner.Api.Data.Model;

namespace UrlShortner.Api.Services.Interface;

internal interface IUrlShortnerService
{
    Task<string> ShortenUrl(string originalUrl);
    Task<string?> GetOriginalUrl(string shortCode);
    Task<IEnumerable<ShortenedUrl>> GetAllUrls();
}

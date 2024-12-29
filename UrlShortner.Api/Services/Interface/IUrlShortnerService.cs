using UrlShortner.Domain.Model;

namespace UrlShortner.Api.Services.Interface;

public interface IUrlShortnerService
{
    Task<string> ShortenUrl(string originalUrl);
    Task<string?> GetOriginalUrl(string shortCode);
    Task<IEnumerable<ShortenedUrl>> GetAllUrls();
}

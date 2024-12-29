using UrlShortner.Api.Data.Model;
using UrlShortner.Api.Services.Interface;

namespace UrlShortner.Api.Services;

internal sealed class UrlShortnerService : IUrlShortnerService
{
    public Task<IEnumerable<ShortenedUrl>> GetAllUrls()
    {
        throw new NotImplementedException();
    }

    public Task<string?> GetOriginalUrl(string shortCode)
    {
        throw new NotImplementedException();
    }

    public Task<string> ShortenUrl(string originalUrl)
    {
        throw new NotImplementedException();
    }
}

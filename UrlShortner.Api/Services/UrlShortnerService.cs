using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using MySqlConnector;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Security.Cryptography;
using UrlShortner.Api.Services.Interface;
using UrlShortner.Domain.Data;
using UrlShortner.Domain.Model;

namespace UrlShortner.Api.Services;

internal sealed class UrlShortnerService(
    AppDbContext dbContext, 
    HybridCache hybridCache,
    IHttpContextAccessor httpContextAccessor,
    ILogger<UrlShortnerService> logger) : IUrlShortnerService
{
    private const int MaxRetries = 3;

    private static readonly Meter Meter = new("UrlShortner.Api");
    private static readonly Counter<int> RedirectsCounter = Meter.CreateCounter<int>(
        "url_shortner.redirects", 
        "The number of successfull redirects");
    private static readonly Counter<int> FailedRedirectsCounter = Meter.CreateCounter<int>(
        "url_shortner.failed_redirects",
        "The number of failed redirects");

    public async Task<IEnumerable<ShortenedUrl>> GetAllUrls()
    {
        var allUrls = await dbContext.ShortenedUrls.OrderByDescending(u => u.CreateAt).ToListAsync();

        return allUrls;
    }

    public async Task<string?> GetOriginalUrl(string shortCode)
    {
        var originalUrl = await hybridCache.GetOrCreateAsync(shortCode, async token =>
        {
            var shortenUrl = await dbContext.ShortenedUrls.Where(u => u.ShortCode == shortCode).FirstOrDefaultAsync(token);

            return shortenUrl?.OriginalUrl;
        });

        if(originalUrl is null)
        {
            FailedRedirectsCounter.Add(1, new TagList { { "short_code", shortCode} });
        }
        else
        {
            await RecordVisit(shortCode);

            RedirectsCounter.Add(1, new TagList { { "short_code", shortCode } });
        }

        return originalUrl;
    }

    public async Task<string> ShortenUrl(string originalUrl)
    {
        for (int attemp = 0; attemp < MaxRetries; attemp++)
        {
            try
            {
                var shortCode = GenerateRandomCode();
                var shortenUrl = new ShortenedUrl(originalUrl: originalUrl, shortCode: shortCode);

                await dbContext.ShortenedUrls.AddAsync(shortenUrl);
                await dbContext.SaveChangesAsync();

                await hybridCache.SetAsync(shortCode, originalUrl);

                return shortCode;
            }
            catch (MySqlException ex) when (ex.SqlState == MySqlErrorCode.DuplicateUnique.ToString())
            {
                if(attemp == MaxRetries)
                {
                    logger.LogError(ex, $"Failed to generate unique short code after {MaxRetries} attempts");

                    throw new InvalidOperationException("Failed to generate unique short code", ex);
                }

                logger.LogWarning($"Short code collision occured. Retrying... (Attempt {attemp+1} of {MaxRetries})");
            }
        }

        throw new InvalidOperationException("Failed to generate unique short code");
    }

    private static string GenerateRandomCode()
    {
        RandomNumberGenerator rng = RandomNumberGenerator.Create();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        const int length = 7;

        var bytes = new byte[length];
        rng.GetBytes(bytes);

        char[] result = new char[length];

        for (int i = 0; i < length; i++)
        {
            result[i] = chars[bytes[i] % chars.Length];
        }

        return new string(result);
    }

    private async Task RecordVisit(string shortCode)
    {
        var httpContext = httpContextAccessor.HttpContext;
        var userAgent = httpContext?.Request.Headers.UserAgent.ToString();
        var referer = httpContext?.Request.Headers.Referer.ToString();

        var shortenUrl = await dbContext.ShortenedUrls.Where(u => u.ShortCode == shortCode).FirstOrDefaultAsync();

        var urlVisit = new UrlVisit()
        {
            Referer = referer ?? "Default",
            UserAgent = userAgent ?? "Default",
            ShortCode = shortCode,
            ShortenedUrlId = shortenUrl!.Id
        };

        await dbContext.UrlVisits.AddAsync(urlVisit);
        await dbContext.SaveChangesAsync();
    }
}

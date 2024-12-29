using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using MySqlConnector;
using System.Security.Cryptography;
using UrlShortner.Api.Services.Interface;
using UrlShortner.Domain.Data;
using UrlShortner.Domain.Model;

namespace UrlShortner.Api.Services;

internal sealed class UrlShortnerService(
    AppDbContext context, 
    HybridCache hybridCache,
    ILogger<UrlShortnerService> logger) : IUrlShortnerService
{
    private const int MaxRetries = 3;

    public async Task<IEnumerable<ShortenedUrl>> GetAllUrls()
    {
        var allUrls = await context.ShortenedUrls.OrderByDescending(u => u.CreateAt).ToListAsync();

        return allUrls;
    }

    public async Task<string?> GetOriginalUrl(string shortCode)
    {
        var originalUrl = await hybridCache.GetOrCreateAsync(shortCode, async token =>
        {
            var shortenUrl = await context.ShortenedUrls.Where(u => u.ShortCode == shortCode).FirstOrDefaultAsync(token);

            return shortenUrl?.OriginalUrl ?? null;
        });

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

                await context.ShortenedUrls.AddAsync(shortenUrl);
                await context.SaveChangesAsync();

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
}

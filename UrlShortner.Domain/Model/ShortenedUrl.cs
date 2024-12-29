using System.ComponentModel.DataAnnotations;

namespace UrlShortner.Domain.Model;

public sealed class ShortenedUrl(string originalUrl, string shortCode)
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string ShortCode { get; set; } = shortCode;
    [Required]
    public string OriginalUrl { get; set; } = originalUrl;
    public DateTime CreateAt { get; set; } = DateTime.UtcNow;
}

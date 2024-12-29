using System.ComponentModel.DataAnnotations;

namespace UrlShortner.Api.Data.Model;

internal sealed class ShortenedUrl
{
    [Key]
    public int Id { get; set; }
    [Required]
    public string ShortCode { get; set; } = string.Empty;
    [Required]
    public string OriginalUrl { get; set; } = string.Empty;
    public DateTime CreateAt { get; set; } = DateTime.UtcNow;
}

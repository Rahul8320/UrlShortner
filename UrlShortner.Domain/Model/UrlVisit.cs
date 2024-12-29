using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UrlShortner.Domain.Model;

public sealed class UrlVisit
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string ShortCode { get; set; } = string.Empty;

    [Required]
    public int ShortenedUrlId { get; set; }
    public DateTime VisitedAt { get; set; } = DateTime.UtcNow;
    public string UserAgent { get; set; } = string.Empty;
    public string Referer { get; set; } = string.Empty;

    public ShortenedUrl? ShortenedUrl { get; set; }
}

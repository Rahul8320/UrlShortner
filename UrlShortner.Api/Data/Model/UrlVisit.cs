using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace UrlShortner.Api.Data.Model;

internal sealed class UrlVisit
{
    [Key]
    public int Id { get; set; }
    [Required]
    [ForeignKey("ShortenedUrl")]
    public string ShortCode { get; set; } = string.Empty;
    public DateTime VisitedAt { get; set; } = DateTime.UtcNow;
    public string UserAgent { get; set; } = string.Empty;
    public string Referer { get; set; } = string.Empty;

    public ShortenedUrl? ShortenedUrl { get; set; }
}

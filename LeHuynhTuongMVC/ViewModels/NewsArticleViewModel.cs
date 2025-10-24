using BusinessObjects.Models;
using System.ComponentModel.DataAnnotations;

namespace LeHuynhTuongMVC.ViewModels;

public class NewsArticleViewModel
{
    public string NewsArticleId { get; set; } = DateTime.Now.ToString("yyyyMMddHHmmss");

    [Required(ErrorMessage = "Title is required")]
    [StringLength(400, ErrorMessage = "Title cannot exceed 400 characters")]
    public string NewsTitle { get; set; } = null!;

    [Required(ErrorMessage = "Headline is required")]
    [StringLength(150, ErrorMessage = "Headline cannot exceed 150 characters")]
    public string Headline { get; set; } = null!;

    [Required(ErrorMessage = "Content is required")]
    [StringLength(4000, ErrorMessage = "Content cannot exceed 4000 characters")]
    public string NewsContent { get; set; } = null!;

    [StringLength(400, ErrorMessage = "Source cannot exceed 400 characters")]
    public string? NewsSource { get; set; }

    [Required(ErrorMessage = "Category is required")]
    public short CategoryId { get; set; }

    [Display(Name = "Published")]
    public bool NewsStatus { get; set; }

    public DateTime? CreatedDate { get; set; }
    public DateTime? ModifiedDate { get; set; }
    public short? CreatedById { get; set; }
    public short? UpdatedById { get; set; }

    // Navigation properties
    public Category? Category { get; set; }
    public SystemAccount? CreatedBy { get; set; }
    public List<int> SelectedTagIds { get; set; } = new();
    public List<Tag> AvailableTags { get; set; } = new();
    public List<Category> AvailableCategories { get; set; } = new();
}

public class ReportViewModel
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public List<NewsArticle> Articles { get; set; } = new();
    public int TotalArticles { get; set; }
    public int PublishedArticles { get; set; }
    public int DraftArticles { get; set; }
    public Dictionary<string, int> ArticlesByCategory { get; set; } = new();
    public Dictionary<string, int> ArticlesByAuthor { get; set; } = new();
} 
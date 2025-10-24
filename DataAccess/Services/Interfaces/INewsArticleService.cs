using BusinessObjects.Models;

namespace DataAccessObjects.Services.Interfaces;

public interface INewsArticleService
{
    Task<NewsArticle?> GetByIdAsync(string id);
    Task<IEnumerable<NewsArticle>> GetAllAsync();
    Task<IEnumerable<NewsArticle>> GetActiveAsync();
    Task<IEnumerable<NewsArticle>> GetByCategoryAsync(short categoryId);
    Task<IEnumerable<NewsArticle>> GetByAuthorAsync(short authorId);
    Task<IEnumerable<NewsArticle>> GetByTagAsync(int tagId);
    Task<NewsArticle> CreateAsync(NewsArticle article, IEnumerable<int> tagIds);
    Task<NewsArticle> UpdateAsync(NewsArticle article, IEnumerable<int> tagIds);
    Task DeleteAsync(string id);
    Task<IEnumerable<NewsArticle>> SearchAsync(string searchTerm);
    Task<IEnumerable<NewsArticle>> GetNewsStatisticsAsync(DateTime startDate, DateTime endDate);
    Task<bool> ExistsAsync(string id);
    Task<IEnumerable<Tag>> GetAllTagsAsync();
} 
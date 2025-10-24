
using BusinessObjects;
using BusinessObjects.Models;
using DataAccessObjects.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects.Repositories;

public class NewsArticleRepository : INewsArticleRepository
{
    private readonly FunewsManagementContext _context;

    public NewsArticleRepository(FunewsManagementContext context)
    {
        _context = context;
    }

    public async Task<NewsArticle?> GetByIdAsync(string id)
    {
        return await _context.NewsArticles
            .Include(n => n.Category)
            .Include(n => n.CreatedBy)
            .Include(n => n.Tags)
            .FirstOrDefaultAsync(n => n.NewsArticleId == id);
    }

    public async Task<IEnumerable<NewsArticle>> GetAllAsync()
    {
        return await _context.NewsArticles
            .Include(n => n.Category)
            .Include(n => n.CreatedBy)
            .Include(n => n.Tags)
            .ToListAsync();
    }

    public async Task<IEnumerable<NewsArticle>> GetByCategoryAsync(short categoryId)
    {
        return await _context.NewsArticles
            .Include(n => n.Category)
            .Include(n => n.CreatedBy)
            .Include(n => n.Tags)
            .Where(n => n.CategoryId == categoryId)
            .ToListAsync();
    }

    public async Task<IEnumerable<NewsArticle>> GetByAuthorAsync(short authorId)
    {
        return await _context.NewsArticles
            .Include(n => n.Category)
            .Include(n => n.CreatedBy)
            .Include(n => n.Tags)
            .Where(n => n.CreatedById == authorId)
            .ToListAsync();
    }

    public async Task<IEnumerable<NewsArticle>> GetByTagAsync(int tagId)
    {
        return await _context.NewsArticles
            .Include(n => n.Category)
            .Include(n => n.CreatedBy)
            .Include(n => n.Tags)
            .Where(n => n.Tags.Any(t => t.TagId == tagId))
            .ToListAsync();
    }

    public async Task<NewsArticle> AddAsync(NewsArticle article)
    {
        await _context.NewsArticles.AddAsync(article);
        await _context.SaveChangesAsync();
        return article;
    }

    public async Task<NewsArticle> UpdateAsync(NewsArticle article)
    {
        _context.NewsArticles.Update(article);
        await _context.SaveChangesAsync();
        return article;
    }

    public async Task DeleteAsync(string id)
    {
        var article = await GetByIdAsync(id);
        if (article != null)
        {
            _context.NewsArticles.Remove(article);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(string id)
    {
        return await _context.NewsArticles.AnyAsync(x => x.NewsArticleId == id);
    }

    public async Task<IEnumerable<Tag>> GetAllTagsAsync()
    {
        return await _context.Tags.ToListAsync();
    }
} 
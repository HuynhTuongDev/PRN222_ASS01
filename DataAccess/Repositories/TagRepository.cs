using BusinessObjects;
using BusinessObjects.Models;
using DataAccessObjects.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects.Repositories;

public class TagRepository : ITagRepository
{
    private readonly FunewsManagementContext _context;

    public TagRepository(FunewsManagementContext context)
    {
        _context = context;
    }

    public async Task<Tag?> GetByIdAsync(int id)
    {
        return await _context.Tags
            .Include(t => t.NewsArticles)
            .FirstOrDefaultAsync(t => t.TagId == id);
    }

    public async Task<IEnumerable<Tag>> GetAllAsync()
    {
        return await _context.Tags
            .Include(t => t.NewsArticles)
            .ToListAsync();
    }

    public async Task<Tag> AddAsync(Tag tag)
    {
        await _context.Tags.AddAsync(tag);
        await _context.SaveChangesAsync();
        return tag;
    }

    public async Task<Tag> UpdateAsync(Tag tag)
    {
        _context.Tags.Update(tag);
        await _context.SaveChangesAsync();
        return tag;
    }

    public async Task DeleteAsync(int id)
    {
        var tag = await GetByIdAsync(id);
        if (tag != null)
        {
            _context.Tags.Remove(tag);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(int id)
    {
        return await _context.Tags.AnyAsync(t => t.TagId == id);
    }

    public async Task<bool> ExistsByNameAsync(string name)
    {
        return await _context.Tags.AnyAsync(t => t.TagName == name);
    }
} 
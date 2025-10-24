using BusinessObjects;
using BusinessObjects.Models;
using DataAccessObjects.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly FunewsManagementContext _context;

    public CategoryRepository(FunewsManagementContext context)
    {
        _context = context;
    }

    public async Task<Category?> GetByIdAsync(short id)
    {
        return await _context.Categories
            .Include(c => c.ParentCategory)
            .Include(c => c.InverseParentCategory)
            .FirstOrDefaultAsync(c => c.CategoryId == id);
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _context.Categories
            .Include(c => c.ParentCategory)
            .Include(c => c.InverseParentCategory)
            .ToListAsync();
    }

    public async Task<IEnumerable<Category>> GetSubCategoriesAsync(short parentId)
    {
        return await _context.Categories
            .Include(c => c.ParentCategory)
            .Include(c => c.InverseParentCategory)
            .Where(c => c.ParentCategoryId == parentId)
            .ToListAsync();
    }

    public async Task<Category> AddAsync(Category category)
    {
        await _context.Categories.AddAsync(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task<Category> UpdateAsync(Category category)
    {
        _context.Categories.Update(category);
        await _context.SaveChangesAsync();
        return category;
    }

    public async Task DeleteAsync(short id)
    {
        var category = await GetByIdAsync(id);
        if (category != null)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(short id)
    {
        return await _context.Categories.AnyAsync(c => c.CategoryId == id);
    }

    public async Task<bool> HasSubCategoriesAsync(short id)
    {
        return await _context.Categories.AnyAsync(c => c.ParentCategoryId == id);
    }
} 
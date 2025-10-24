using BusinessObjects.Models;

namespace DataAccessObjects.Repositories.Interfaces;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(short id);
    Task<IEnumerable<Category>> GetAllAsync();
    Task<IEnumerable<Category>> GetSubCategoriesAsync(short parentId);
    Task<Category> AddAsync(Category category);
    Task<Category> UpdateAsync(Category category);
    Task DeleteAsync(short id);
    Task<bool> ExistsAsync(short id);
    Task<bool> HasSubCategoriesAsync(short id);
} 
using BusinessObjects.Models;
using DataAccessObjects.Repositories.Interfaces;
using DataAccessObjects.Services.Interfaces;

namespace DataAccessObjects.Services;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly INewsArticleRepository _newsRepository;

    public CategoryService(ICategoryRepository categoryRepository, INewsArticleRepository newsRepository)
    {
        _categoryRepository = categoryRepository;
        _newsRepository = newsRepository;
    }

    public async Task<Category?> GetByIdAsync(short id)
    {
        return await _categoryRepository.GetByIdAsync(id);
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _categoryRepository.GetAllAsync();
    }

    public async Task<IEnumerable<Category>> GetActiveAsync()
    {
        var allCategories = await _categoryRepository.GetAllAsync();
        return allCategories.Where(c => c.IsActive == true);
    }

    public async Task<IEnumerable<Category>> GetSubCategoriesAsync(short parentId)
    {
        return await _categoryRepository.GetSubCategoriesAsync(parentId);
    }

    public async Task<Category> CreateAsync(Category category)
    {
        // Validate parent category if specified
        if (category.ParentCategoryId.HasValue)
        {
            if (!await _categoryRepository.ExistsAsync(category.ParentCategoryId.Value))
            {
                throw new InvalidOperationException("Parent category does not exist");
            }
        }

        return await _categoryRepository.AddAsync(category);
    }

    public async Task<Category> UpdateAsync(Category category)
    {
        var existingCategory = await _categoryRepository.GetByIdAsync(category.CategoryId);
        if (existingCategory == null)
        {
            throw new InvalidOperationException("Category not found");
        }

        // Validate parent category if specified
        if (category.ParentCategoryId.HasValue)
        {
            if (!await _categoryRepository.ExistsAsync(category.ParentCategoryId.Value))
            {
                throw new InvalidOperationException("Parent category does not exist");
            }

            // Prevent circular reference
            if (category.ParentCategoryId == category.CategoryId)
            {
                throw new InvalidOperationException("Category cannot be its own parent");
            }
        }

        return await _categoryRepository.UpdateAsync(category);
    }

    public async Task DeleteAsync(short id)
    {
        if (!await _categoryRepository.ExistsAsync(id))
        {
            throw new InvalidOperationException("Category not found");
        }

        // Check if category has any news articles
        if (await HasNewsArticlesAsync(id))
        {
            throw new InvalidOperationException("Cannot delete category that has news articles");
        }

        await _categoryRepository.DeleteAsync(id);
    }

    public async Task<IEnumerable<Category>> SearchAsync(string searchTerm)
    {
        var allCategories = await _categoryRepository.GetAllAsync();
        return allCategories.Where(c =>
            c.CategoryName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            c.CategoryDesciption.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<bool> ExistsAsync(short id)
    {
        return await _categoryRepository.ExistsAsync(id);
    }

    public async Task<bool> HasNewsArticlesAsync(short id)
    {
        var newsInCategory = await _newsRepository.GetByCategoryAsync(id);
        return newsInCategory.Any();
    }
} 
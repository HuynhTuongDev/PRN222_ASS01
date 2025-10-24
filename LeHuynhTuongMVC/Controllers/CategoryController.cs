using BusinessObjects.Models;
using DataAccessObjects.Services.Interfaces;
using LeHuynhTuongMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;


namespace Web.Controllers;

public class CategoryController : Controller
{
    private readonly ICategoryService _categoryService;

    public CategoryController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var accountId = HttpContext.Session.GetInt32("AccountId");
        if (!accountId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        var role = HttpContext.Session.GetInt32("AccountRole");
        if (role != 1 && role != 3) // Staff and Admin can view categories
        {
            TempData["Error"] = "You do not have permission to access this page.";
            return RedirectToAction("Index", "Home");
        }

        var categories = await _categoryService.GetAllAsync();
        return View(categories);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var accountId = HttpContext.Session.GetInt32("AccountId");
        if (!accountId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        var role = HttpContext.Session.GetInt32("AccountRole");
        if (role != 1) // Only Staff can create categories
        {
            TempData["Error"] = "You do not have permission to access this page.";
            return RedirectToAction("Index", "Home");
        }

        var categories = await _categoryService.GetAllAsync();
        var model = new CategoryViewModel
        {
            AvailableParentCategories = categories.ToList()
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CategoryViewModel model)
    {
        var accountId = HttpContext.Session.GetInt32("AccountId");
        if (!accountId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        var role = HttpContext.Session.GetInt32("AccountRole");
        if (role != 1) // Only Staff can create categories
        {
            TempData["Error"] = "You do not have permission to access this page.";
            return RedirectToAction("Index", "Home");
        }

        if (!ModelState.IsValid)
        {
            model.AvailableParentCategories = (await _categoryService.GetAllAsync()).ToList();
            return View(model);
        }

        try
        {
            var category = new Category
            {
                CategoryName = model.CategoryName,
                CategoryDesciption = model.CategoryDesciption,
                ParentCategoryId = model.ParentCategoryId,
                IsActive = model.IsActive
            };

            await _categoryService.CreateAsync(category);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            model.AvailableParentCategories = (await _categoryService.GetAllAsync()).ToList();
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var accountId = HttpContext.Session.GetInt32("AccountId");
        if (!accountId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        var role = HttpContext.Session.GetInt32("AccountRole");
        if (role != 1) // Only Staff can edit categories
        {
            TempData["Error"] = "You do not have permission to access this page.";
            return RedirectToAction("Index", "Home");
        }

        var category = await _categoryService.GetByIdAsync((short)id);
        if (category == null)
        {
            return NotFound();
        }

        var categories = await _categoryService.GetAllAsync();
        var model = new CategoryViewModel
        {
            CategoryId = category.CategoryId,
            CategoryName = category.CategoryName,
            CategoryDesciption = category.CategoryDesciption,
            ParentCategoryId = category.ParentCategoryId,
            IsActive = category.IsActive ?? false,
            AvailableParentCategories = categories.Where(c => c.CategoryId != id).ToList() // Exclude current category from parent options
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(CategoryViewModel model)
    {
        var accountId = HttpContext.Session.GetInt32("AccountId");
        if (!accountId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        var role = HttpContext.Session.GetInt32("AccountRole");
        if (role != 1) // Only Staff can edit categories
        {
            TempData["Error"] = "You do not have permission to access this page.";
            return RedirectToAction("Index", "Home");
        }

        if (!ModelState.IsValid)
        {
            model.AvailableParentCategories = (await _categoryService.GetAllAsync()).Where(c => c.CategoryId != model.CategoryId).ToList();
            return View(model);
        }

        try
        {
            var category = await _categoryService.GetByIdAsync(model.CategoryId);
            if (category == null)
            {
                return NotFound();
            }

            category.CategoryName = model.CategoryName;
            category.CategoryDesciption = model.CategoryDesciption;
            category.ParentCategoryId = model.ParentCategoryId;
            category.IsActive = model.IsActive;

            await _categoryService.UpdateAsync(category);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            model.AvailableParentCategories = (await _categoryService.GetAllAsync()).Where(c => c.CategoryId != model.CategoryId).ToList();
            return View(model);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var accountId = HttpContext.Session.GetInt32("AccountId");
        if (!accountId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        var role = HttpContext.Session.GetInt32("AccountRole");
        if (role != 1) // Only Staff can delete categories
        {
            TempData["Error"] = "You do not have permission to access this page.";
            return RedirectToAction("Index", "Home");
        }

        try
        {
            // Check if category exists
            var category = await _categoryService.GetByIdAsync((short)id);
            if (category == null)
            {
                TempData["Error"] = "Category not found";
                return RedirectToAction(nameof(Index));
            }

            // Check if category has any news articles
            if (await _categoryService.HasNewsArticlesAsync((short)id))
            {
                TempData["Error"] = "Cannot delete this category because it is associated with one or more news articles. Please remove or reassign these articles first.";
                return RedirectToAction(nameof(Index));
            }

            await _categoryService.DeleteAsync((short)id);
            TempData["Success"] = "Category deleted successfully";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }
} 
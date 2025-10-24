using BusinessObjects.Models;
using DataAccessObjects.Services.Interfaces;
using LeHuynhTuongMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

public class NewsArticleController : Controller
{
    private readonly INewsArticleService _newsArticleService;
    private readonly ICategoryService _categoryService;
    private readonly ISystemAccountService _accountService;

    public NewsArticleController(
        INewsArticleService newsArticleService,
        ICategoryService categoryService,
        ISystemAccountService accountService)
    {
        _newsArticleService = newsArticleService;
        _categoryService = categoryService;
        _accountService = accountService;
    }

    [HttpGet]
    public async Task<IActionResult> PublicView(string searchString, int? categoryId, int? page)
    {
        var pageSize = 10;
        var pageNumber = page ?? 1;

        var articles = await _newsArticleService.GetAllAsync();
        
        // Only show active articles
        articles = articles.Where(a => a.NewsStatus == true);

        // Apply search filter
        if (!string.IsNullOrEmpty(searchString))
        {
            articles = articles.Where(a => 
                a.NewsTitle.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                a.Headline.Contains(searchString, StringComparison.OrdinalIgnoreCase));
        }

        // Apply category filter
        if (categoryId.HasValue)
        {
            articles = articles.Where(a => a.CategoryId == categoryId);
        }

        // Get categories for filter dropdown
        ViewBag.Categories = await _categoryService.GetAllAsync();
        ViewBag.CurrentSearch = searchString;
        ViewBag.CurrentCategory = categoryId;

        return View(articles.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList());
    }

    [HttpGet]
    public async Task<IActionResult> PublicDetails(int id)
    {
        var article = await _newsArticleService.GetByIdAsync(id.ToString());
        if (article == null || article.NewsStatus != true)
        {
            return NotFound();
        }

        return View(article);
    }

    [HttpGet]
    public async Task<IActionResult> Index(string searchString, int? categoryId, int? page)
    {
        var accountId = HttpContext.Session.GetInt32("AccountId");
        if (!accountId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        var role = HttpContext.Session.GetInt32("AccountRole");
        var pageSize = 10;
        var pageNumber = page ?? 1;

        var articles = await _newsArticleService.GetAllAsync();
        
        // Filter by role
        if (role == 1) // Staff can view all articles but can only edit their own
        {
            // No filtering needed for viewing
        }
        else if (role == 2) // Lecturer
        {
            articles = articles.Where(a => a.NewsStatus == true); // Only published articles
        }
        else if (role == 3) // Admin can view all articles
        {
            // No filtering needed for admin - can view all articles
        }

        // Apply search filter
        if (!string.IsNullOrEmpty(searchString))
        {
            articles = articles.Where(a => 
                a.NewsTitle.Contains(searchString, StringComparison.OrdinalIgnoreCase) ||
                a.Headline.Contains(searchString, StringComparison.OrdinalIgnoreCase));
        }

        // Apply category filter
        if (categoryId.HasValue)
        {
            articles = articles.Where(a => a.CategoryId == categoryId);
        }

        // Get categories for filter dropdown
        ViewBag.Categories = await _categoryService.GetAllAsync();
        ViewBag.CurrentSearch = searchString;
        ViewBag.CurrentCategory = categoryId;

        return View(articles.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList());
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var accountId = HttpContext.Session.GetInt32("AccountId");
        if (!accountId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        var article = await _newsArticleService.GetByIdAsync(id.ToString());
        if (article == null)
        {
            return NotFound();
        }

        var role = HttpContext.Session.GetInt32("AccountRole");
        if (role == 2 && article.NewsStatus != true) // Lecturer can only view published articles
        {
            TempData["Error"] = "You can only view published articles.";
            return RedirectToAction("Index");
        }
        // Staff (role 1) and Admin (role 3) can view all articles
        // Admin (role == 3) can view all articles without restrictions

        return View(article);
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
        if (role != 1) // Only Staff can create articles (Admin cannot)
        {
            TempData["Error"] = "You do not have permission to create articles.";
            return RedirectToAction("Index", "Home");
        }

        var categories = await _categoryService.GetAllAsync();
        var activeCategories = categories.Where(c => c.IsActive == true).ToList();
        var tags = await _newsArticleService.GetAllTagsAsync();
        var model = new NewsArticleViewModel
        {
            AvailableCategories = activeCategories,
            AvailableTags = tags.ToList()
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Create(NewsArticleViewModel model)
    {
        var accountId = HttpContext.Session.GetInt32("AccountId");
        if (!accountId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        var role = HttpContext.Session.GetInt32("AccountRole");
        if (role != 1) // Only Staff can create articles (Admin cannot)
        {
            TempData["Error"] = "You do not have permission to create articles.";
            return RedirectToAction("Index", "Home");
        }

        if (!ModelState.IsValid)
        {
            // Add validation errors to ViewBag
            ViewBag.ValidationErrors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            model.AvailableCategories = (await _categoryService.GetAllAsync()).ToList();
            model.AvailableTags = (await _newsArticleService.GetAllTagsAsync()).ToList();
            return View(model);
        }

        try
        {
            var article = new NewsArticle
            {
                NewsArticleId = model.NewsArticleId,
                NewsTitle = model.NewsTitle,
                Headline = model.Headline,
                NewsContent = model.NewsContent,
                NewsSource = model.NewsSource,
                CategoryId = model.CategoryId,
                NewsStatus = model.NewsStatus,
                CreatedById = (short?)accountId.Value,
                CreatedDate = DateTime.Now
            };

            await _newsArticleService.CreateAsync(article, model.SelectedTagIds);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            model.AvailableCategories = (await _categoryService.GetAllAsync()).ToList();
            model.AvailableTags = (await _newsArticleService.GetAllTagsAsync()).ToList();
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(string id)
    {
        var accountId = HttpContext.Session.GetInt32("AccountId");
        if (!accountId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        var role = HttpContext.Session.GetInt32("AccountRole");
        if (role != 1) // Only Staff can edit articles (Admin cannot)
        {
            TempData["Error"] = "You do not have permission to edit articles.";
            return RedirectToAction("Index", "Home");
        }

        var article = await _newsArticleService.GetByIdAsync(id);
        if (article == null)
        {
            return NotFound();
        }

        // Staff can edit all articles, no ownership restriction

        var model = new NewsArticleViewModel
        {
            NewsArticleId = article.NewsArticleId,
            NewsTitle = article.NewsTitle,
            Headline = article.Headline,
            NewsContent = article.NewsContent,
            NewsSource = article.NewsSource,
            CategoryId = (short)article.CategoryId!,
            NewsStatus = article.NewsStatus ?? false,
            AvailableCategories = (await _categoryService.GetAllAsync()).ToList(),
            AvailableTags = (await _newsArticleService.GetAllTagsAsync()).ToList(),
            SelectedTagIds = article.Tags.Select(t => t.TagId).ToList()
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(NewsArticleViewModel model)
    {
        var accountId = HttpContext.Session.GetInt32("AccountId");
        if (!accountId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        var role = HttpContext.Session.GetInt32("AccountRole");
        if (role != 1) // Only Staff can edit articles (Admin cannot)
        {
            TempData["Error"] = "You do not have permission to edit articles.";
            return RedirectToAction("Index", "Home");
        }

        if (!ModelState.IsValid)
        {
            model.AvailableCategories = (await _categoryService.GetAllAsync()).ToList();
            return View(model);
        }

        try
        {
            var article = await _newsArticleService.GetByIdAsync(model.NewsArticleId);
            if (article == null)
            {
                return NotFound();
            }

            // Staff can edit all articles, no ownership restriction

            article.NewsTitle = model.NewsTitle;
            article.Headline = model.Headline;
            article.NewsContent = model.NewsContent;
            article.NewsSource = model.NewsSource;
            article.CategoryId = model.CategoryId;
            article.NewsStatus = model.NewsStatus;
            article.UpdatedById = (short?)accountId;
            article.ModifiedDate = DateTime.Now;

            await _newsArticleService.UpdateAsync(article, model.SelectedTagIds);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            model.AvailableCategories = (await _categoryService.GetAllAsync()).ToList();
            return View(model);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Delete(string id)
    {
        var accountId = HttpContext.Session.GetInt32("AccountId");
        if (!accountId.HasValue)
        {
            return RedirectToAction("Login", "Account");
        }

        var role = HttpContext.Session.GetInt32("AccountRole");
        if (role != 1) // Only Staff can delete articles (Admin cannot)
        {
            TempData["Error"] = "You do not have permission to delete articles.";
            return RedirectToAction("Index", "Home");
        }

        try
        {
            var article = await _newsArticleService.GetByIdAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            // Staff can delete all articles, no ownership restriction

            await _newsArticleService.DeleteAsync(id);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> History(string searchString)
    {
        var userId = HttpContext.Session.GetInt32("AccountId");
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        ViewBag.CurrentFilter = searchString;
        var articles = await _newsArticleService.GetByAuthorAsync((short)userId.Value);

        if (!string.IsNullOrEmpty(searchString))
        {
            articles = articles.Where(x => x.NewsTitle.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        return View(articles);
    }

    [HttpGet]
    public async Task<IActionResult> Report(DateTime? startDate, DateTime? endDate)
    {
        // Check if user is admin
        if (HttpContext.Session.GetInt32("AccountRole") != 3)
        {
            TempData["Error"] = "You do not have permission to access this page.";
            return RedirectToAction("Index");
        }

        var articles = (await _newsArticleService.GetAllAsync()).ToList();

        // Apply date filters
        if (startDate.HasValue)
        {
            articles = articles.Where(x => x.CreatedDate >= startDate.Value).ToList();
        }

        if (endDate.HasValue)
        {
            articles = articles.Where(x => x.CreatedDate <= endDate.Value.AddDays(1)).ToList();
        }

        // Sort by created date in descending order
        articles = articles.OrderByDescending(x => x.CreatedDate).ToList();

        var model = new ReportViewModel
        {
            StartDate = startDate,
            EndDate = endDate,
            Articles = articles,
            TotalArticles = articles.Count,
            PublishedArticles = articles.Count(x => x.NewsStatus == true),
            DraftArticles = articles.Count(x => x.NewsStatus == false),
            ArticlesByCategory = articles
                .GroupBy(x => x.Category?.CategoryName ?? "Uncategorized")
                .ToDictionary(g => g.Key, g => g.Count()),
            ArticlesByAuthor = articles
                .GroupBy(x => x.CreatedBy?.AccountName ?? "Unknown")
                .ToDictionary(g => g.Key, g => g.Count())
        };

        return View(model);
    }
} 
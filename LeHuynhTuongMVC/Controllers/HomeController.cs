using DataAccessObjects.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

public class HomeController : Controller
{
    private readonly INewsArticleService _newsArticleService;
    private readonly ICategoryService _categoryService;

    public HomeController(INewsArticleService newsArticleService, ICategoryService categoryService)
    {
        _newsArticleService = newsArticleService;
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
        var articles = await _newsArticleService.GetAllAsync();

        // Filter articles based on role
        if (role == 1) // Staff can view all articles
        {
            // No filtering needed for viewing
        }
        else if (role == 2) // Lecturer
        {
            articles = articles.Where(a => a.NewsStatus == true); // Only published articles
        }

        // Get latest articles
        var latestArticles = articles.OrderByDescending(a => a.CreatedDate).Take(5).ToList();

        // Get categories with article counts
        var categories = await _categoryService.GetAllAsync();
        var activeCategories = categories.Where(c => c.IsActive == true).ToList();
        var categoryStats = activeCategories.Select(c => new
        {
            Category = c,
            ArticleCount = articles.Count(a => a.CategoryId == c.CategoryId)
        }).ToList();

        ViewBag.LatestArticles = latestArticles;
        ViewBag.CategoryStats = categoryStats;
        ViewBag.UserRole = role;

        return View();
    }

    [HttpGet]
    public IActionResult Error()
    {
        return View();
    }
}

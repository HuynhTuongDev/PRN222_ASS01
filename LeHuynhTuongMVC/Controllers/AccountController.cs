using BusinessObjects.Models;
using DataAccessObjects.Services.Interfaces;
using LeHuynhTuongMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace LeHuynhTuongMVC.Controllers;

public class AccountController : Controller
{
    private readonly ISystemAccountService _accountService;
    private readonly IConfiguration _configuration;

    public AccountController(ISystemAccountService accountService, IConfiguration configuration)
    {
        _accountService = accountService;
        _configuration = configuration;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var isValid = await _accountService.ValidateLoginAsync(model.Email, model.Password);
        if (!isValid)
        {
            ModelState.AddModelError("", "Invalid email or password");
            return View(model);
        }

        var account = await _accountService.GetByEmailAsync(model.Email);
        if (account == null)
        {
            ModelState.AddModelError("", "Account not found");
            return View(model);
        }

        // Set session
        HttpContext.Session.SetInt32("AccountId", account.AccountId);
        HttpContext.Session.SetString("AccountName", account.AccountName ?? "");
        HttpContext.Session.SetInt32("AccountRole", account.AccountRole ?? 0);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Login");
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!IsAdmin())
        {
            return RedirectToAction("Login");
        }

        var accounts = await _accountService.GetAllAsync();
        return View(accounts);
    }

    [HttpGet]
    public IActionResult Create()
    {
        if (!IsAdmin())
        {
            return RedirectToAction("Login");
        }

        return View(new SystemAccountViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Create(SystemAccountViewModel model)
    {
        if (!IsAdmin())
        {
            return RedirectToAction("Login");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var account = new SystemAccount
            {
                AccountName = model.AccountName,
                AccountEmail = model.AccountEmail,
                AccountPassword = model.AccountPassword,
                AccountRole = model.AccountRole
            };

            await _accountService.CreateAsync(account);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(short id)
    {
        if (!IsAdmin())
        {
            return RedirectToAction("Login");
        }

        var account = await _accountService.GetByIdAsync(id);
        if (account == null)
        {
            return NotFound();
        }

        var model = new SystemAccountViewModel
        {
            AccountId = account.AccountId,
            AccountName = account.AccountName ?? "",
            AccountEmail = account.AccountEmail ?? "",
            AccountPassword = account.AccountPassword ?? "",
            AccountRole = account.AccountRole ?? 0
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(SystemAccountViewModel model)
    {
        if (!IsAdmin())
        {
            return RedirectToAction("Login");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var account = await _accountService.GetByIdAsync(model.AccountId);
            if (account == null)
            {
                return NotFound();
            }

            account.AccountName = model.AccountName;
            account.AccountEmail = model.AccountEmail;
            account.AccountRole = model.AccountRole;
            
            // Only update password if a new one is provided
            if (!string.IsNullOrEmpty(model.AccountPassword))
            {
                account.AccountPassword = model.AccountPassword;
            }

            await _accountService.UpdateAsync(account);
            TempData["Success"] = "Account updated successfully";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Delete(short id)
    {
        if (!IsAdmin())
        {
            return RedirectToAction("Login");
        }

        try
        {
            await _accountService.DeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var accountId = HttpContext.Session.GetInt32("AccountId");
        if (!accountId.HasValue)
        {
            return RedirectToAction("Login");
        }

        var account = await _accountService.GetByIdAsync((short)accountId.Value);
        if (account == null)
        {
            return NotFound();
        }

        var model = new ProfileUpdateViewModel
        {
            AccountId = account.AccountId,
            AccountName = account.AccountName ?? "",
            AccountEmail = account.AccountEmail ?? "",
            AccountRole = account.AccountRole ?? 0
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> UpdateProfile(ProfileUpdateViewModel model)
    {
        var accountId = HttpContext.Session.GetInt32("AccountId");
        if (!accountId.HasValue || accountId.Value != model.AccountId)
        {
            return RedirectToAction("Login");
        }

        if (!ModelState.IsValid)
        {
            return View("Profile", model);
        }

        try
        {
            var account = await _accountService.GetByIdAsync(model.AccountId);
            if (account == null)
            {
                return NotFound();
            }

            account.AccountName = model.AccountName;
            // Only update password if a new one is provided
            if (!string.IsNullOrEmpty(model.AccountPassword))
            {
                account.AccountPassword = model.AccountPassword;
            }

            await _accountService.UpdateAsync(account);
            TempData["Success"] = "Profile updated successfully";
            return RedirectToAction(nameof(Profile));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View("Profile", model);
        }
    }

    private bool IsAdmin()
    {
        var role = HttpContext.Session.GetInt32("AccountRole");
        return role.HasValue && role.Value == 3; // 3 is Admin role
    }
} 
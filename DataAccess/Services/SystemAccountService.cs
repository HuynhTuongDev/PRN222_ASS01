using BusinessObjects.Models;
using DataAccessObjects.Repositories.Interfaces;
using DataAccessObjects.Services.Interfaces;

namespace DataAccessObjects.Services;

public class SystemAccountService : ISystemAccountService
{
    private readonly ISystemAccountRepository _accountRepository;

    public SystemAccountService(ISystemAccountRepository accountRepository)
    {
        _accountRepository = accountRepository;
    }

    public async Task<SystemAccount?> GetByIdAsync(short id)
    {
        return await _accountRepository.GetByIdAsync(id);
    }

    public async Task<SystemAccount?> GetByEmailAsync(string email)
    {
        return await _accountRepository.GetByEmailAsync(email);
    }

    public async Task<IEnumerable<SystemAccount>> GetAllAsync()
    {
        return await _accountRepository.GetAllAsync();
    }

    public async Task<SystemAccount> CreateAsync(SystemAccount account)
    {
        if (await _accountRepository.ExistsByEmailAsync(account.AccountEmail))
        {
            throw new InvalidOperationException("Email already exists");
        }

        return await _accountRepository.AddAsync(account);
    }

    public async Task<SystemAccount> UpdateAsync(SystemAccount account)
    {
        var existingAccount = await _accountRepository.GetByIdAsync(account.AccountId);
        if (existingAccount == null)
        {
            throw new InvalidOperationException("Account not found");
        }

        // Check if email is being changed and if it already exists
        if (existingAccount.AccountEmail != account.AccountEmail &&
            await _accountRepository.ExistsByEmailAsync(account.AccountEmail))
        {
            throw new InvalidOperationException("Email already exists");
        }

        return await _accountRepository.UpdateAsync(account);
    }

    public async Task DeleteAsync(short id)
    {
        if (!await _accountRepository.ExistsAsync(id))
        {
            throw new InvalidOperationException("Account not found");
        }

        await _accountRepository.DeleteAsync(id);
    }

    public async Task<bool> ValidateLoginAsync(string email, string password)
    {
        var account = await _accountRepository.GetByEmailAsync(email);
        return account != null && account.AccountPassword == password;
    }

    public async Task<IEnumerable<SystemAccount>> SearchAsync(string searchTerm)
    {
        var accounts = await _accountRepository.GetAllAsync();
        return accounts.Where(a =>
            a.AccountName?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true ||
            a.AccountEmail?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true);
    }

    public async Task<bool> ExistsAsync(short id)
    {
        return await _accountRepository.ExistsAsync(id);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _accountRepository.ExistsByEmailAsync(email);
    }
} 
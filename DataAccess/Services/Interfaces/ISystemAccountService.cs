using BusinessObjects.Models;

namespace DataAccessObjects.Services.Interfaces;

public interface ISystemAccountService
{
    Task<SystemAccount?> GetByIdAsync(short id);
    Task<SystemAccount?> GetByEmailAsync(string email);
    Task<IEnumerable<SystemAccount>> GetAllAsync();
    Task<SystemAccount> CreateAsync(SystemAccount account);
    Task<SystemAccount> UpdateAsync(SystemAccount account);
    Task DeleteAsync(short id);
    Task<bool> ValidateLoginAsync(string email, string password);
    Task<IEnumerable<SystemAccount>> SearchAsync(string searchTerm);
    Task<bool> ExistsAsync(short id);
    Task<bool> ExistsByEmailAsync(string email);
} 
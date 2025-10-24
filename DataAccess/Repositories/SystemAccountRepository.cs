using BusinessObjects;
using BusinessObjects.Models;
using DataAccessObjects.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DataAccessObjects.Repositories;

public class SystemAccountRepository : ISystemAccountRepository
{
    private readonly FunewsManagementContext _context;

    public SystemAccountRepository(FunewsManagementContext context)
    {
        _context = context;
    }

    public async Task<SystemAccount?> GetByIdAsync(short id)
    {
        return await _context.SystemAccounts.FindAsync(id);
    }

    public async Task<SystemAccount?> GetByEmailAsync(string email)
    {
        return await _context.SystemAccounts
            .FirstOrDefaultAsync(a => a.AccountEmail == email);
    }

    public async Task<IEnumerable<SystemAccount>> GetAllAsync()
    {
        return await _context.SystemAccounts.ToListAsync();
    }

    public async Task<SystemAccount> AddAsync(SystemAccount account)
    {
        await _context.SystemAccounts.AddAsync(account);
        await _context.SaveChangesAsync();
        return account;
    }

    public async Task<SystemAccount> UpdateAsync(SystemAccount account)
    {
        _context.SystemAccounts.Update(account);
        await _context.SaveChangesAsync();
        return account;
    }

    public async Task DeleteAsync(short id)
    {
        var account = await GetByIdAsync(id);
        if (account != null)
        {
            _context.SystemAccounts.Remove(account);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(short id)
    {
        return await _context.SystemAccounts.AnyAsync(a => a.AccountId == id);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _context.SystemAccounts.AnyAsync(a => a.AccountEmail == email);
    }
} 
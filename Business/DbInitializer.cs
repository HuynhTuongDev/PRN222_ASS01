using Microsoft.EntityFrameworkCore;

namespace BusinessObjects;

public class DbInitializer
{
    private readonly FunewsManagementContext _context;

    public DbInitializer(FunewsManagementContext context)
    {
        _context = context;
    }

    public async Task InitializeAsync()
    {
        // Ensure database is created
        await _context.Database.EnsureCreatedAsync();
    }
} 
using Microsoft.EntityFrameworkCore;
using StudyCompanion.Shared.Models;

namespace StudyCompanion.Data;

public class PostgresDbContext : DbContext
{
    public DbSet<User> Player { get; set; }
}
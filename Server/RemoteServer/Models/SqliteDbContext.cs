using Microsoft.EntityFrameworkCore;

namespace RemoteServer;

public class SqliteDbContext : DbContext
{
    protected readonly IConfiguration Configuration;

    public SqliteDbContext(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(Configuration.GetConnectionString("DbPath"));
    }

    public DbSet<SyncLogHead> syncLogHeads { get; set; }
    public DbSet<SyncLogFile> syncLogFiles { get; set; }
}

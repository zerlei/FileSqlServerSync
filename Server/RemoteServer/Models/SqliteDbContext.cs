using Microsoft.EntityFrameworkCore;

namespace RemoteServer.Models;

public class SqliteDbContext(IConfiguration configuration) : DbContext
{
    protected readonly IConfiguration Configuration = configuration;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite(Configuration.GetConnectionString("DbPath"));
    }

    public DbSet<SyncLogHead> SyncLogHeads { get; set; }
    public DbSet<SyncLogFile> SyncLogFiles { get; set; }
    public DbSet<SyncGitCommit> SyncGitCommits { get; set; }
}

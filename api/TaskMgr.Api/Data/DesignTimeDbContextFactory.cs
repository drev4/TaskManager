using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TaskMgr.Api.Data;

/// <summary>
/// Factory for creating DbContext instances at design time (for migrations)
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<TaskMgrDbContext>
{
    public TaskMgrDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<TaskMgrDbContext>();

        // Use the connection string from appsettings.json
        var connectionString = "Server=(localdb)\\mssqllocaldb;Database=TaskMgrDb;Trusted_Connection=true;MultipleActiveResultSets=true;TrustServerCertificate=true;";

        optionsBuilder.UseSqlServer(connectionString);

        return new TaskMgrDbContext(optionsBuilder.Options);
    }
}

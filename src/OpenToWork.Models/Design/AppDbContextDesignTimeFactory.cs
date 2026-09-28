using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using OpenToWork.Models.Context;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;

namespace OpenToWork.Models.Design;

public class AppDbContextDesignTimeFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // En produccion el despliegue pasa la cadena real por variable de entorno (ver deploy.yml).
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
            ?? "Server=localhost;Port=3306;Database=OpenToWorkDb;User=root;Password=;CharSet=utf8mb4;";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseMySql(connectionString, ServerVersion.Create(8, 0, 36, ServerType.MySql))
            .Options;

        return new AppDbContext(options);
    }
}

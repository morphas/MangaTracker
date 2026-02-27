using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MangaTracker.Api.Data
{
    // Usado pelo EF em tempo de design (migrations / update).
    public class MangaTrackerContextFactory : IDesignTimeDbContextFactory<MangaTrackerDbContext>
    {
        public MangaTrackerDbContext CreateDbContext(string[] args)
        {
            // 1) Começa de onde o comando foi executado
            var dir = new DirectoryInfo(Directory.GetCurrentDirectory());

            // 2) Sobe pastas até achar o csproj da API (isso evita pegar appsettings "errado")
            while (dir != null && !File.Exists(Path.Combine(dir.FullName, "MangaTracker.Api.csproj")))
                dir = dir.Parent;

            if (dir == null)
                throw new InvalidOperationException("Não encontrei MangaTracker.Api.csproj subindo as pastas. Rode o comando dentro da pasta MangaTracker.Api.");

            var basePath = dir.FullName;

            // 3) Carrega config SEM mistério: exatamente da pasta do csproj
            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            var conn = config.GetConnectionString("Default");            

            if (string.IsNullOrWhiteSpace(conn))
                throw new InvalidOperationException("ConnectionStrings:Default não encontrada. Verifique appsettings.json.");

            var optionsBuilder = new DbContextOptionsBuilder<MangaTrackerDbContext>();
            optionsBuilder.UseNpgsql(conn);

            return new MangaTrackerDbContext(optionsBuilder.Options);
        }
    }
}
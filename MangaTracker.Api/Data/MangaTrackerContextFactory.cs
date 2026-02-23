using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MangaTracker.Api.Data
{
    // Esta classe é usada pelo EF apenas em tempo de design (Migrations / Update-Database).
    // Ela precisa ler o appsettings.json para usar a mesma connection string do projeto.
    public class MangaTrackerContextFactory : IDesignTimeDbContextFactory<MangaTrackerDbContext>
    {
        public MangaTrackerDbContext CreateDbContext(string[] args)
        {
            // Garante que estamos apontando para a pasta do projeto da API (onde está o appsettings.json)
            var basePath = Directory.GetCurrentDirectory();

            // Se o comando for executado a partir de outro lugar, tenta “voltar” para achar o appsettings.json
            // (isso evita erro quando o VS executa o comando de uma pasta diferente).
            if (!File.Exists(Path.Combine(basePath, "appsettings.json")))
            {
                // tenta achar a pasta MangaTracker.Api subindo diretórios
                var dir = new DirectoryInfo(basePath);
                while (dir != null && !File.Exists(Path.Combine(dir.FullName, "appsettings.json")))
                    dir = dir.Parent;

                if (dir != null)
                    basePath = dir.FullName;
            }

            var config = new ConfigurationBuilder()
                .SetBasePath(basePath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile("appsettings.Development.json", optional: true, reloadOnChange: false)
                .AddUserSecrets<MangaTrackerContextFactory>(optional: true)
                .AddEnvironmentVariables()
                .Build();

            var conn = config.GetConnectionString("Default");

            if (string.IsNullOrWhiteSpace(conn))
                throw new InvalidOperationException("ConnectionStrings:Default não foi encontrada no appsettings.json.");

            var optionsBuilder = new DbContextOptionsBuilder<MangaTrackerDbContext>();
            optionsBuilder.UseNpgsql(conn);

            return new MangaTrackerDbContext(optionsBuilder.Options);
        }
    }
}
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MangaTracker.Api.Data
{
    // Esta classe serve exclusivamente para as ferramentas de linha de comando (Migrations)
    public class MangaTrackerContextFactory : IDesignTimeDbContextFactory<MangaTrackerDbContext>
    {
        public MangaTrackerDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MangaTrackerDbContext>();

            // Usamos uma string genérica aqui. Ela serve apenas para a ferramenta conseguir 
            // gerar os arquivos de migração sem travar. No Render, valerá a conexão real.
            optionsBuilder.UseNpgsql("Host=localhost;Database=temp;Username=postgres;Password=pass");

            return new MangaTrackerDbContext(optionsBuilder.Options);
        }
    }
}
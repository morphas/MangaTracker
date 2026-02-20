using Microsoft.EntityFrameworkCore;
using MangaTracker.Models;

namespace MangaTracker.Api.Data
{
    public class MangaTrackerDbContext : DbContext
    {
        public MangaTrackerDbContext(DbContextOptions<MangaTrackerDbContext> options) : base(options) { }

        // Suas tabelas no banco de dados
        public DbSet<Manga> Catalogo { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Leitura> Leituras { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuração da chave composta para a tabela de Leitura
            modelBuilder.Entity<Leitura>()
                .HasKey(l => new { l.UsuarioId, l.MangaId });

            base.OnModelCreating(modelBuilder);
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Se o banco não estiver configurado (comum quando rodamos o comando no terminal),
            // a gente define uma string temporária apenas para a ferramenta não travar.
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseNpgsql("Host=localhost;Database=temp;Username=postgres;Password=pass");
            }
        }
    }
}
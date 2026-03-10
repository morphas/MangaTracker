using MangaTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace MangaTracker.Api.Data
{
    public class MangaTrackerDbContext : DbContext
    {
        public MangaTrackerDbContext(DbContextOptions<MangaTrackerDbContext> options) : base(options) { }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Editora> Editoras { get; set; }
        public DbSet<Manga> Catalogo { get; set; }
        public DbSet<Leitura> Leituras { get; set; }
        public DbSet<AdminLog> AdminLogs { get; set; }

        public DbSet<RankingHome> RankingHomes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Leitura>()
                .HasKey(l => new { l.UsuarioId, l.MangaId });

            modelBuilder.Entity<Manga>()
                .Property(m => m.Generos)
                .HasColumnType("text[]");

            base.OnModelCreating(modelBuilder);
        }
    }
}
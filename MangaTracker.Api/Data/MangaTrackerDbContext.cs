using Microsoft.EntityFrameworkCore;
using MangaTracker.Models;

namespace MangaTracker.Api.Data
{
    public class MangaTrackerDbContext : DbContext
    {
        public MangaTrackerDbContext(DbContextOptions<MangaTrackerDbContext> options) : base(options) { }

        public DbSet<Manga> Catalogo { get; set; } = null!;
        public DbSet<Usuario> Usuarios { get; set; } = null!;
        public DbSet<Leitura> Leituras { get; set; } = null!;
        public DbSet<AdminLog> AdminLogs { get; set; } = null!;
        public DbSet<Editora> Editoras { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ✅ Leitura (chave composta)
            modelBuilder.Entity<Leitura>()
                .HasKey(l => new { l.UsuarioId, l.MangaId });

            // ✅ AdminLog
            modelBuilder.Entity<AdminLog>()
                .HasKey(a => a.Id);

            // ✅ Editora (tabela + regras)
            modelBuilder.Entity<Editora>(e =>
            {
                e.ToTable("Editoras");
                e.HasKey(x => x.Id);

                e.Property(x => x.Nome)
                    .HasMaxLength(120)
                    .IsRequired();

                // ⚠️ Aqui era "NomeKey" no seu DbContext, mas no seu model é "Key"
                e.Property(x => x.Key)
                    .HasMaxLength(60)
                    .IsRequired();

                e.HasIndex(x => x.Key)
                    .IsUnique();
            });

            // ✅ Relacionamento: Manga -> Editora (FK opcional)
            modelBuilder.Entity<Manga>()
                .HasOne(m => m.EditoraNav)
                .WithMany()
                .HasForeignKey(m => m.EditoraId)
                .OnDelete(DeleteBehavior.SetNull);

            base.OnModelCreating(modelBuilder);
        }
    }
}
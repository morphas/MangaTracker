using System;

namespace MangaTracker.Models
{
    public class Editora
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Nome { get; set; } = "";

        // chave normalizada para filtros/URL (ex: "panini", "jbc")
        public string Key { get; set; } = "";

        public string? Descricao { get; set; } = null;

        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    }
}
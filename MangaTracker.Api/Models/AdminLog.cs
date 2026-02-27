using System;

namespace MangaTracker.Models
{
    public class AdminLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid AdminId { get; set; }  // quem fez
        public string Acao { get; set; } = ""; // ex: "CATALOGO_CRIAR", "CATALOGO_EDITAR", "CATALOGO_EXCLUIR"
        public string? Detalhes { get; set; } // texto livre com info útil

        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    }
}
using System;

namespace MangaTracker.Models
{
    public class Manga
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Básico (cadastro rápido)
        public string Titulo { get; set; } = "";
        public int? TotalCapitulos { get; set; } = null;

        public bool LancadoNoBrasil { get; set; } = false;
        public string? Editora { get; set; } = null;

        // para filtro por editora
        public string? EditoraKey { get; set; } = null;

        // ===== Detalhes (admin preenche depois) =====
        public string? CapaUrl { get; set; } = null;        // link da imagem
        public string? Descricao { get; set; } = null;

        // Ex.: "Shounen", "Shoujo", "Seinen", "Josei"
        public string? Demografia { get; set; } = null;

        public string? Autor { get; set; } = null;

        // Ano do lançamento original (Japão/país de origem)
        public int? AnoLancamentoOriginal { get; set; } = null;

        // Ano do lançamento no Brasil (se aplicável)
        public int? AnoLancamentoBrasil { get; set; } = null;

        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public Guid? EditoraId { get; set; } = null;
        public Editora? EditoraNav { get; set; } = null;
    }
}
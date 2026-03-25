using System;
using System.Collections.Generic;

namespace MangaTracker.Models
{
    public class Manga
    {
        public Guid Id { get; set; }

        public string Titulo { get; set; } = string.Empty;

        public int TotalCapitulos { get; set; }

        public bool LancadoNoBrasil { get; set; }

        public Guid? EditoraId { get; set; }

        public Editora? EditoraNav { get; set; }

        public string? CapaUrl { get; set; }

        public string? Descricao { get; set; }

        public string? Demografia { get; set; }

        public string? Autor { get; set; }

        public int? AnoLancamentoOriginal { get; set; }

        public int? AnoLancamentoBrasil { get; set; }

        /// <summary>ID do MyAnimeList (Jikan) para evitar duplicatas na importação.</summary>
        public int? MalId { get; set; }

        // ✅ NOVO CAMPO
        public List<string> Generos { get; set; } = new();

        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    }
}
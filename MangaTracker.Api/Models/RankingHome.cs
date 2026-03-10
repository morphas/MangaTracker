using System;

namespace MangaTracker.Models
{
    public class RankingHome
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Tipo { get; set; } = "";

        public Guid MangaId { get; set; }

        public int Posicao { get; set; }

        public int Valor { get; set; }

        public DateTime GeradoEm { get; set; } = DateTime.UtcNow;
    }
}
using System;

namespace MangaTracker.Models
{
    public class Manga
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Titulo { get; set; } = "";
                        
        public int? TotalCapitulos { get; set; } = null;

        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
    }
}

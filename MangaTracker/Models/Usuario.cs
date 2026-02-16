using System;

namespace MangaTracker.Models
{
    public class Usuario
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nome { get; set; } = "";
        public DateTime CriadoEm { get; set; } = DateTime.Now;
    }
}

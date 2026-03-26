using System;

namespace MangaTracker.Models
{
    public class Usuario
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public string Nome { get; set; } = "";

        public string Email { get; set; } = "";

        public string SenhaHash { get; set; } = "";

        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

        public bool EhAdmin { get; set; } = false;
    }
}
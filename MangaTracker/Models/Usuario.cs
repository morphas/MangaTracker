using System;

namespace MangaTracker.Models
{
    public class Usuario
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Nome { get; set; } = "";

        // ADICIONE ESTAS DUAS LINHAS:
        public string Email { get; set; } = "";
        public string Senha { get; set; } = "";

        public DateTime CriadoEm { get; set; } = DateTime.UtcNow;
        public bool EhAdmin { get; set; } = false;
    }
}
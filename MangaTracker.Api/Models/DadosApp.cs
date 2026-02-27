using System;
using System.Collections.Generic;

namespace MangaTracker.Models
{
    public class DadosApp
    {
        public List<Manga> Catalogo { get; set; } = new();
        public List<Leitura> MinhaLista { get; set; } = new();

        public List<Usuario> Usuarios { get; set; } = new();
        public Guid UsuarioAtualId { get; set; }

        public DateTime SalvoEm { get; set; } = DateTime.Now;
        public int Versao { get; set; } = 1;
    }
}

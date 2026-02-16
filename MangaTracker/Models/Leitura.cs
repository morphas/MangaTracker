using System;

namespace MangaTracker.Models
{
    public class Leitura
    {
        public Guid UsuarioId { get; set; }

        public Guid MangaId { get; set; }
        public StatusLeitura Status { get; set; } = StatusLeitura.PretendoLer;

        public int CapituloAtual { get; set; } = 0;
        public DateTime? UltimaLeituraEm { get; set; } = null;
        public DateTime AdicionadoEm { get; set; } = DateTime.Now;
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using MangaTracker.Models;

namespace MangaTracker.Services
{
    public class BibliotecaService
    {
        private readonly List<Manga> _catalogo = new();
        private readonly List<Leitura> _minhaLista = new();

        // =========================
        // CATÁLOGO
        // =========================
        public IReadOnlyList<Manga> ListarCatalogo()
            => _catalogo;

        public Manga? BuscarMangaPorId(Guid id)
            => _catalogo.FirstOrDefault(m => m.Id == id);

        public Manga? BuscarMangaPorTitulo(string titulo)
        {
            string t = NormalizarTitulo(titulo);
            return _catalogo.FirstOrDefault(m => NormalizarTitulo(m.Titulo) == t);
        }

        public bool MangaExisteNoCatalogo(string titulo)
            => BuscarMangaPorTitulo(titulo) is not null;

        public Manga CadastrarNoCatalogo(string titulo, int? totalCapitulos = null)
        {
            string t = titulo.Trim();

            if (MangaExisteNoCatalogo(t))
                throw new InvalidOperationException("Esse mangá já existe no catálogo.");

            var manga = new Manga
            {
                Titulo = t,
                TotalCapitulos = totalCapitulos
            };

            _catalogo.Add(manga);
            return manga;
        }

        public void DefinirTotalCapitulos(Guid mangaId, int totalCapitulos)
        {
            Manga? manga = BuscarMangaPorId(mangaId);
            if (manga is null) return;

            manga.TotalCapitulos = totalCapitulos;
        }

        // =========================
        // MINHA LISTA (LEITURA)
        // =========================
        public IReadOnlyList<(Manga Manga, Leitura Leitura)> ListarMinhaLista()
        {
            // Junta leitura com o manga do catálogo
            return _minhaLista
                .Select(l =>
                {
                    var manga = BuscarMangaPorId(l.MangaId);
                    return (manga, l);
                })
                .Where(x => x.manga is not null)
                .Select(x => (x.manga!, x.l))
                .ToList();
        }

        public IReadOnlyList<(Manga Manga, Leitura Leitura)> ListarMinhaListaPorStatus(StatusLeitura status)
        {
            return ListarMinhaLista()
                .Where(x => x.Leitura.Status == status)
                .ToList();
        }

        public bool EstaNaMinhaLista(Guid mangaId)
            => _minhaLista.Any(l => l.MangaId == mangaId);

        public void AdicionarNaMinhaLista(Guid mangaId, StatusLeitura status, int? capituloAtual = null)
        {
            if (BuscarMangaPorId(mangaId) is null)
                throw new InvalidOperationException("Mangá não encontrado no catálogo.");

            if (EstaNaMinhaLista(mangaId))
                throw new InvalidOperationException("Esse mangá já está na sua lista de leitura.");

            var leitura = new Leitura
            {
                MangaId = mangaId,
                Status = status
            };

            // Regras padrão de progresso conforme status
            if (status == StatusLeitura.PretendoLer)
            {
                leitura.CapituloAtual = 0;
                leitura.UltimaLeituraEm = null;
            }
            else
            {
                leitura.CapituloAtual = capituloAtual ?? 0;
                leitura.UltimaLeituraEm = DateTime.Now;
            }

            // Se já adiciona como Concluído e o catálogo tiver total, força capituloAtual = total
            var manga = BuscarMangaPorId(mangaId);
            if (status == StatusLeitura.Concluido && manga?.TotalCapitulos is not null)
                leitura.CapituloAtual = manga.TotalCapitulos.Value;

            _minhaLista.Add(leitura);
        }

        public void AtualizarLeitura(Guid mangaId, int capituloAtual, StatusLeitura? status = null)
        {
            var leitura = _minhaLista.FirstOrDefault(l => l.MangaId == mangaId);
            if (leitura is null) return;

            if (capituloAtual < 0)
                capituloAtual = 0;

            leitura.CapituloAtual = capituloAtual;
            leitura.UltimaLeituraEm = DateTime.Now;

            if (status.HasValue)
                leitura.Status = status.Value;

            // Se tiver total no catálogo e atingiu, marca concluído automaticamente
            var manga = BuscarMangaPorId(mangaId);
            if (manga?.TotalCapitulos is not null && leitura.CapituloAtual >= manga.TotalCapitulos.Value)
            {
                leitura.Status = StatusLeitura.Concluido;
                leitura.CapituloAtual = manga.TotalCapitulos.Value;
            }
        }

        // =========================
        // Helpers
        // =========================
        private static string NormalizarTitulo(string titulo)
            => titulo.Trim().ToUpperInvariant();
    }
}

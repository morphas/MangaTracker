using System;
using System.Collections.Generic;
using MangaTracker.Models;

namespace MangaTracker.Services
{
    public interface IBibliotecaService
    {
        // =========================
        // CATÁLOGO
        // =========================
        IReadOnlyList<Manga> ListarCatalogo();
        Manga? BuscarMangaPorId(Guid id);
        Manga? BuscarMangaPorTitulo(string titulo);
        bool MangaExisteNoCatalogo(string titulo);
        Manga CadastrarNoCatalogo(string titulo, bool lancadoNoBrasil, string? editora);
        void DefinirTotalCapitulos(Guid mangaId, int? totalCapitulos);
        Manga AtualizarMangaDoCatalogo(Guid mangaId, string titulo, bool lancadoNoBrasil, string? editora);

        Manga AtualizarDetalhesManga( Guid mangaId, string? capaUrl, string? descricao, string? demografia, string? autor, int? anoLancamentoOriginal, int? anoLancamentoBrasil);
        void RemoverMangaDoCatalogo(Guid mangaId);

        // =========================
        // MINHA LISTA (LEITURAS)
        // =========================
        IReadOnlyList<(Manga Manga, Leitura Leitura)> ListarMinhaLista();
        bool EstaNaMinhaLista(Guid mangaId);
        void AdicionarNaMinhaLista(Guid mangaId, StatusLeitura status, int? capituloAtual = null);
        void AtualizarLeitura(Guid mangaId, int capituloAtual, StatusLeitura? status = null);
        void RemoverDaMinhaLista(Guid mangaId);

        // =========================
        // USUÁRIOS
        // =========================
        Usuario? ObterUsuarioLogado();
        IReadOnlyList<Usuario> ListarUsuarios();
        bool DefinirUsuarioAtual(Guid usuarioId);
        Usuario ValidarLogin(string identificador, string senha);

        // =========================
        // PERSISTÊNCIA (compat)
        // =========================
        void CarregarDados();
        void SalvarDados();
        string CaminhoDoArquivoDeDados();
        void CadastrarNovoUsuario(string nome, string email, string senha);
    }
}
using System;
using System.Collections.Generic;
using MangaTracker.Models;

namespace MangaTracker.Services
{
    // Resultado paginado genérico
    public record PagedResult<T>(IReadOnlyList<T> Items, int Total, int Page, int PageSize);

    public interface IBibliotecaService
    {
        // =========================
        // CATÁLOGO
        // =========================
        IReadOnlyList<Manga> ListarCatalogo();
        PagedResult<Manga> ListarCatalogoPaginado(bool? lancadoNoBrasil, Guid? editoraId, string? q, int page, int pageSize);

        Manga? BuscarMangaPorId(Guid id);
        Manga? BuscarMangaPorTitulo(string titulo);
        bool MangaExisteNoCatalogo(string titulo);

        Manga CadastrarNoCatalogo(string titulo, bool lancadoNoBrasil, Guid? editoraId);

        Manga AtualizarMangaDoCatalogo(Guid mangaId, string titulo, bool lancadoNoBrasil, Guid? editoraId);

        Manga AtualizarDetalhesManga(
            Guid mangaId,
            string? capaUrl,
            string? descricao,
            string? demografia,
            string? autor,
            int? anoLancamentoOriginal,
            int? anoLancamentoBrasil
        );

        void RemoverMangaDoCatalogo(Guid mangaId);

        void DefinirTotalCapitulos(Guid mangaId, int? totalCapitulos);

        // =========================
        // EDITORAS
        // =========================
        IReadOnlyList<Editora> ListarEditoras();
        Editora? BuscarEditoraPorId(Guid id);
        Editora CriarEditora(string nome, string? descricao);
        Editora AtualizarEditora(Guid id, string nome, string? descricao);

        // Regra: NÃO pode excluir se houver mangá vinculado
        void RemoverEditora(Guid id);

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
        void CadastrarNovoUsuario(string nome, string email, string senha);

        // =========================
        // PERSISTÊNCIA (compat)
        // =========================
        void CarregarDados();
        void SalvarDados();
        string CaminhoDoArquivoDeDados();
    }
}
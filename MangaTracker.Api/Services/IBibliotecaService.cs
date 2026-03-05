using System;
using System.Collections.Generic;
using MangaTracker.Models;

namespace MangaTracker.Services
{
    public interface IBibliotecaService
    {
        // Boot
        void CarregarDados();
        void SalvarDados();
        string CaminhoDoArquivoDeDados();

        // Usuários
        Usuario? ObterUsuarioLogado();
        IReadOnlyList<Usuario> ListarUsuarios();
        bool DefinirUsuarioAtual(Guid usuarioId);
        Usuario ValidarLogin(string identificador, string senha);
        void CadastrarNovoUsuario(string nome, string email, string senha);

        // Editoras
        IReadOnlyList<Editora> ListarEditoras();
        Editora? BuscarEditoraPorId(Guid id);
        Editora CriarEditora(string nome, string? descricao);
        Editora AtualizarEditora(Guid id, string nome, string? descricao);
        void RemoverEditora(Guid id);

        // Catálogo
        IReadOnlyList<Manga> ListarCatalogo();
        PagedResult<Manga> ListarCatalogoPaginado(bool? lancadoNoBrasil, Guid? editoraId, string? q, int page, int pageSize);
        Manga? BuscarMangaPorId(Guid id);
        Manga? BuscarMangaPorTitulo(string titulo);
        bool MangaExisteNoCatalogo(string titulo);

        Manga CadastrarNoCatalogo(string titulo, bool lancadoNoBrasil, Guid? editoraId);
        Manga AtualizarMangaDoCatalogo(Guid mangaId, string titulo, bool lancadoNoBrasil, Guid? editoraId);

        // ✅ Detalhes avançados (agora com generos como lista)
        Manga AtualizarDetalhesManga(
            Guid mangaId,
            string? capaUrl,
            string? descricao,
            string? demografia,
            string? autor,
            int? anoLancamentoOriginal,
            int? anoLancamentoBrasil,
            List<string>? generos
        );

        void RemoverMangaDoCatalogo(Guid mangaId);
        void DefinirTotalCapitulos(Guid mangaId, int? totalCapitulos);

        // Minha lista
        IReadOnlyList<(Manga Manga, Leitura Leitura)> ListarMinhaLista();
        bool EstaNaMinhaLista(Guid mangaId);
        void AdicionarNaMinhaLista(Guid mangaId, StatusLeitura status, int? capituloAtual = null);
        void AtualizarLeitura(Guid mangaId, int capituloAtual, StatusLeitura? status = null);
        void RemoverDaMinhaLista(Guid mangaId);
    }
}
using System;
using System.Collections.Generic;
using MangaTracker.Models;

namespace MangaTracker.Services
{
    public interface IBibliotecaService
    {
        // Catálogo
        IReadOnlyList<Manga> ListarCatalogo();
        Manga? BuscarMangaPorId(Guid id);
        Manga? BuscarMangaPorTitulo(string titulo);
        bool MangaExisteNoCatalogo(string titulo);
        Manga CadastrarNoCatalogo(string titulo, int? totalCapitulos = null);
        void DefinirTotalCapitulos(Guid mangaId, int? totalCapitulos);

        // Minha lista (Leituras)
        IReadOnlyList<(Manga Manga, Leitura Leitura)> ListarMinhaLista();
        IReadOnlyList<(Manga Manga, Leitura Leitura)> ListarMinhaListaPorStatus(StatusLeitura status);
        bool EstaNaMinhaLista(Guid mangaId);
        void AdicionarNaMinhaLista(Guid mangaId, StatusLeitura status, int? capituloAtual = null);
        void AtualizarLeitura(Guid mangaId, int capituloAtual, StatusLeitura? status = null);

        // Usuários
        IReadOnlyList<Usuario> ListarUsuarios();
        Usuario? UsuarioAtual();
        Usuario CriarUsuario(string nome);
        bool DefinirUsuarioAtual(Guid usuarioId);

        // Persistência
        void CarregarDados();
        void SalvarDados();
        string CaminhoDoArquivoDeDados();
    }
}

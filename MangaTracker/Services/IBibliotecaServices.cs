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
        Manga CadastrarNoCatalogo(string titulo, int? totalCapitulos = null);
        void DefinirTotalCapitulos(Guid mangaId, int? totalCapitulos);

        // =========================
        // MINHA LISTA (LEITURAS)
        // =========================
        IReadOnlyList<(Manga Manga, Leitura Leitura)> ListarMinhaLista();
        bool EstaNaMinhaLista(Guid mangaId);
        void AdicionarNaMinhaLista(Guid mangaId, StatusLeitura status, int? capituloAtual = null);
        void AtualizarLeitura(Guid mangaId, int capituloAtual, StatusLeitura? status = null);

        // =========================
        // USUÁRIOS
        // =========================
        // Este é o método principal que o seu Controller usa para checar se você é Admin
        Usuario? ObterUsuarioLogado();

        IReadOnlyList<Usuario> ListarUsuarios();
        bool DefinirUsuarioAtual(Guid usuarioId);

        // =========================
        // PERSISTÊNCIA (JSON)
        // =========================
        void CarregarDados();
        void SalvarDados();
        string CaminhoDoArquivoDeDados();

        void CadastrarNovoUsuario(string nome, string email, string senha);

        // ADICIONE ESTA LINHA ABAIXO:
        Usuario ValidarLogin(string identificador, string senha);
    }
}
    

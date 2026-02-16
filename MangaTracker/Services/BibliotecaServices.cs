using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using MangaTracker.Models;

namespace MangaTracker.Services
{
    public class BibliotecaService
    {
        private readonly List<Manga> _catalogo = new();
        private readonly List<Leitura> _minhaLista = new();
        private readonly List<Usuario> _usuarios = new();
        private readonly JsonStorage _storage;

        private Guid _usuarioAtualId = Guid.Empty;

        private void AutoSalvar()
        {
            SalvarDados();
        }


        public BibliotecaService()
        {
            string pasta = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "MangaTracker"
            );

            string arquivo = Path.Combine(pasta, "dados.json");
            _storage = new JsonStorage(arquivo);

            _usuarioAtualId = Guid.NewGuid();
        }

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
            AutoSalvar();
            return manga;
        }

        public void DefinirTotalCapitulos(Guid mangaId, int? totalCapitulos)
        {
            Manga? manga = BuscarMangaPorId(mangaId);
            if (manga is null) return;

            // Se vier um número, ele precisa ser >= 1
            if (totalCapitulos.HasValue && totalCapitulos.Value < 1)
                throw new InvalidOperationException("Total de capítulos deve ser >= 1.");

            manga.TotalCapitulos = totalCapitulos;
            AutoSalvar();
        }

        // =========================
        // MINHA LISTA (LEITURA)
        // =========================

        public IReadOnlyList<(Manga Manga, Leitura Leitura)> ListarMinhaLista()
        {
            return LeiturasDoUsuarioAtual()
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
    => LeiturasDoUsuarioAtual().Any(l => l.MangaId == mangaId);


        public void AdicionarNaMinhaLista(Guid mangaId, StatusLeitura status, int? capituloAtual = null)
        {
            if (BuscarMangaPorId(mangaId) is null)
                throw new InvalidOperationException("Mangá não encontrado no catálogo.");

            if (EstaNaMinhaLista(mangaId))
                throw new InvalidOperationException("Esse mangá já está na sua lista de leitura.");

            var leitura = new Leitura
            {
                UsuarioId = _usuarioAtualId,
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
            AutoSalvar();
        }

        public void AtualizarLeitura(Guid mangaId, int capituloAtual, StatusLeitura? status = null)
        {
            var leitura = LeiturasDoUsuarioAtual().FirstOrDefault(l => l.MangaId == mangaId);
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
            AutoSalvar();
        }

        // =========================
        // PERSISTÊNCIA (JSON)
        // =========================
        public void CarregarDados()
        {
            var dados = _storage.Carregar();

            _usuarios.Clear();
            _usuarios.AddRange(dados.Usuarios);

            if (dados.UsuarioAtualId != Guid.Empty)
                _usuarioAtualId = dados.UsuarioAtualId;

            // Se não tem usuários ainda, cria um padrão
            if (_usuarios.Count == 0)
            {
                var padrao = new Usuario { Nome = "Rafael" };
                _usuarios.Add(padrao);
                _usuarioAtualId = padrao.Id;
            }

            _catalogo.Clear();
            _catalogo.AddRange(dados.Catalogo);

            _minhaLista.Clear();
            _minhaLista.AddRange(dados.MinhaLista);

            // MIGRAÇÃO: leituras antigas sem UsuarioId vão para o usuário atual
            bool migrou = false;

            for (int i = 0; i < _minhaLista.Count; i++)
            {
                if (_minhaLista[i].UsuarioId == Guid.Empty)
                {
                    _minhaLista[i].UsuarioId = _usuarioAtualId;
                    migrou = true;
                }
            }

            if (migrou)
                SalvarDados();

        }


        public void SalvarDados()
        {
            var dados = new DadosApp
            {
                Usuarios = _usuarios.ToList(),
                UsuarioAtualId = _usuarioAtualId,
                Catalogo = _catalogo.ToList(),
                MinhaLista = _minhaLista.ToList()
            };

            _storage.Salvar(dados);
        }


        public string CaminhoDoArquivoDeDados()
            => _storage.CaminhoArquivo;

        // =========================
        // Helpers
        // =========================
        private static string NormalizarTitulo(string titulo)
            => titulo.Trim().ToUpperInvariant();

        public IReadOnlyList<Usuario> ListarUsuarios()
     => _usuarios;

        public Usuario? BuscarUsuarioPorId(Guid id)
            => _usuarios.FirstOrDefault(u => u.Id == id);

        public Usuario? BuscarUsuarioPorNome(string nome)
        {
            string n = (nome ?? "").Trim();
            if (string.IsNullOrWhiteSpace(n)) return null;

            return _usuarios.FirstOrDefault(u =>
                string.Equals(u.Nome.Trim(), n, StringComparison.CurrentCultureIgnoreCase));
        }

        public bool UsuarioExiste(string nome)
            => BuscarUsuarioPorNome(nome) is not null;

        public Usuario CriarUsuario(string nome)
        {
            string n = (nome ?? "").Trim();
            if (string.IsNullOrWhiteSpace(n))
                throw new InvalidOperationException("Nome de usuário é obrigatório.");

            if (UsuarioExiste(n))
                throw new InvalidOperationException("Já existe um usuário com esse nome.");

            var user = new Usuario { Nome = n };
            _usuarios.Add(user);

            // se for o primeiro usuário, vira o atual automaticamente
            if (_usuarioAtualId == Guid.Empty)
                _usuarioAtualId = user.Id;

            return user;
        }

        public Usuario? UsuarioAtual()
            => BuscarUsuarioPorId(_usuarioAtualId);

        public bool DefinirUsuarioAtual(Guid usuarioId)
        {
            if (BuscarUsuarioPorId(usuarioId) is null)
                return false;

            _usuarioAtualId = usuarioId;
            return true;
        }

        private IEnumerable<Leitura> LeiturasDoUsuarioAtual()
    => _minhaLista.Where(l => l.UsuarioId == _usuarioAtualId);

    }
}

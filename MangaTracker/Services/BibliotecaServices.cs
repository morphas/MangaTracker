using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using MangaTracker.Models;

namespace MangaTracker.Services
{
    public class BibliotecaService : IBibliotecaService
    {
        private readonly List<Manga> _catalogo = new();
        private readonly List<Leitura> _minhaLista = new();
        private readonly List<Usuario> _usuarios = new();
        private readonly JsonStorage _storage;
        private readonly object _lock = new();

        private Guid _usuarioAtualId = Guid.Empty;

        public BibliotecaService()
        {
            // Busca o dados.json na mesma pasta onde a aplicação está rodando
            string arquivo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dados.json");
            _storage = new JsonStorage(arquivo);
        }

        private void AutoSalvar() => SalvarDados();

        // =========================
        // LOGIN / USUÁRIO ATUAL
        // =========================
        public Usuario? ObterUsuarioLogado()
        {
            lock (_lock)
            {
                return _usuarios.FirstOrDefault(u => u.Id == _usuarioAtualId);
            }
        }

        public IReadOnlyList<Usuario> ListarUsuarios()
        {
            lock (_lock)
            {
                return _usuarios.ToList();
            }
        }

        public Usuario? BuscarUsuarioPorId(Guid id)
        {
            lock (_lock)
            {
                return _usuarios.FirstOrDefault(u => u.Id == id);
            }
        }

        public bool DefinirUsuarioAtual(Guid usuarioId)
        {
            lock (_lock)
            {
                if (_usuarios.All(u => u.Id != usuarioId)) return false;
                _usuarioAtualId = usuarioId;
                AutoSalvar();
                return true;
            }
        }

        // =========================
        // CATÁLOGO
        // =========================
        public IReadOnlyList<Manga> ListarCatalogo()
        {
            lock (_lock)
            {
                return _catalogo.ToList(); // evita alguém mexer no _catalogo fora
            }
        }

        public Manga? BuscarMangaPorId(Guid id)
        {
            lock (_lock)
            {
                return _catalogo.FirstOrDefault(m => m.Id == id);
            }
        }

        public Manga? BuscarMangaPorTitulo(string titulo)
        {
            lock (_lock)
            {
                string t = NormalizarTitulo(titulo);
                return _catalogo.FirstOrDefault(m => NormalizarTitulo(m.Titulo) == t);
            }
        }

        public bool MangaExisteNoCatalogo(string titulo)
            => BuscarMangaPorTitulo(titulo) is not null;

        public Manga CadastrarNoCatalogo(string titulo, bool lancadoNoBrasil, string? editora)
        {
            lock (_lock)
            {
                if (string.IsNullOrWhiteSpace(titulo))
                    throw new Exception("Título é obrigatório.");

                var t = titulo.Trim();

                // Se for lançado no Brasil, editora vira obrigatória
                if (lancadoNoBrasil && string.IsNullOrWhiteSpace(editora))
                    throw new Exception("Editora é obrigatória quando o mangá é lançado no Brasil.");

                // Se NÃO for lançado, editora tem que ficar nula (limpa)
                if (!lancadoNoBrasil)
                    editora = null;

                // Verifica se já existe (por título)
                if (_catalogo.Any(m => m.Titulo.Equals(t, StringComparison.CurrentCultureIgnoreCase)))
                    throw new Exception("Esse mangá já existe no catálogo.");

                var manga = new Manga
                {
                    Titulo = t,
                    LancadoNoBrasil = lancadoNoBrasil,
                    Editora = editora?.Trim(),
                    EditoraKey = NormalizeEditoraKey(editora)
                };

                _catalogo.Add(manga);
                AutoSalvar();
                return manga;
            }
        }

        public Manga AtualizarMangaDoCatalogo(Guid id, string titulo, bool lancadoNoBrasil, string? editora)
        {
            lock (_lock)
            {
                if (id == Guid.Empty)
                    throw new Exception("ID inválido.");

                if (string.IsNullOrWhiteSpace(titulo))
                    throw new Exception("Título é obrigatório.");

                if (lancadoNoBrasil && string.IsNullOrWhiteSpace(editora))
                    throw new Exception("Informe a editora se o mangá for lançado no Brasil.");

                var manga = _catalogo.FirstOrDefault(m => m.Id == id);
                if (manga is null)
                    throw new Exception("Mangá não encontrado.");

                manga.Titulo = titulo.Trim();
                manga.LancadoNoBrasil = lancadoNoBrasil;

                if (lancadoNoBrasil)
                {
                    manga.Editora = editora?.Trim();
                    manga.EditoraKey = NormalizeEditoraKey(manga.Editora);
                }
                else
                {
                    manga.Editora = null;
                    manga.EditoraKey = null;
                }

                AutoSalvar();
                return manga;
            }
        }

        public void RemoverMangaDoCatalogo(Guid id)
        {
            lock (_lock)
            {
                if (id == Guid.Empty)
                    throw new Exception("ID inválido.");

                var manga = _catalogo.FirstOrDefault(m => m.Id == id);
                if (manga is null)
                    throw new Exception("Mangá não encontrado.");

                // Remove do catálogo
                _catalogo.Remove(manga);

                // Remove também das leituras (pra não deixar lixo na lista dos usuários)
                _minhaLista.RemoveAll(l => l.MangaId == id);

                AutoSalvar();
            }
        }

        public void DefinirTotalCapitulos(Guid mangaId, int? totalCapitulos)
        {
            lock (_lock)
            {
                var manga = _catalogo.FirstOrDefault(m => m.Id == mangaId);
                if (manga is null) return;

                if (totalCapitulos.HasValue && totalCapitulos.Value < 1)
                    throw new InvalidOperationException("Total de capítulos deve ser >= 1.");

                manga.TotalCapitulos = totalCapitulos;
                AutoSalvar();
            }
        }

        // =========================
        // MINHA LISTA (LEITURA)
        // =========================
        public IReadOnlyList<(Manga Manga, Leitura Leitura)> ListarMinhaLista()
        {
            lock (_lock)
            {
                return LeiturasDoUsuarioAtual()
                    .Select(l =>
                    {
                        var manga = _catalogo.FirstOrDefault(m => m.Id == l.MangaId);
                        return (manga, l);
                    })
                    .Where(x => x.manga is not null)
                    .Select(x => (x.manga!, x.l))
                    .ToList();
            }
        }

        public void AdicionarNaMinhaLista(Guid mangaId, StatusLeitura status, int? capituloAtual = null)
        {
            lock (_lock)
            {
                if (_usuarioAtualId == Guid.Empty)
                    throw new Exception("Faça login primeiro.");

                if (_catalogo.All(m => m.Id != mangaId))
                    throw new InvalidOperationException("Mangá não encontrado no catálogo.");

                if (EstaNaMinhaLista(mangaId))
                    throw new InvalidOperationException("Esse mangá já está na sua lista de leitura.");

                var leitura = new Leitura
                {
                    UsuarioId = _usuarioAtualId,
                    MangaId = mangaId,
                    Status = status
                };

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

                var manga = _catalogo.FirstOrDefault(m => m.Id == mangaId);
                if (status == StatusLeitura.Concluido && manga?.TotalCapitulos is not null)
                    leitura.CapituloAtual = manga.TotalCapitulos.Value;

                _minhaLista.Add(leitura);
                AutoSalvar();
            }
        }

        public void RemoverDaMinhaLista(Guid mangaId)
        {
            lock (_lock)
            {
                if (_usuarioAtualId == Guid.Empty)
                    throw new Exception("Faça login primeiro.");

                var leitura = _minhaLista.FirstOrDefault(l =>
                    l.UsuarioId == _usuarioAtualId && l.MangaId == mangaId);

                if (leitura == null)
                    throw new Exception("Esse mangá não está na sua lista.");

                _minhaLista.Remove(leitura);
                AutoSalvar(); // ✅ faltava
            }
        }

        public void AtualizarLeitura(Guid mangaId, int capituloAtual, StatusLeitura? status = null)
        {
            lock (_lock)
            {
                var leitura = LeiturasDoUsuarioAtual().FirstOrDefault(l => l.MangaId == mangaId);
                if (leitura is null) return;

                leitura.CapituloAtual = capituloAtual < 0 ? 0 : capituloAtual;
                leitura.UltimaLeituraEm = DateTime.Now;

                if (status.HasValue)
                    leitura.Status = status.Value;

                var manga = _catalogo.FirstOrDefault(m => m.Id == mangaId);
                if (manga?.TotalCapitulos is not null && leitura.CapituloAtual >= manga.TotalCapitulos.Value)
                {
                    leitura.Status = StatusLeitura.Concluido;
                    leitura.CapituloAtual = manga.TotalCapitulos.Value;
                }

                AutoSalvar();
            }
        }

        public bool EstaNaMinhaLista(Guid mangaId)
        {
            lock (_lock)
            {
                return LeiturasDoUsuarioAtual().Any(l => l.MangaId == mangaId);
            }
        }

        private IEnumerable<Leitura> LeiturasDoUsuarioAtual()
            => _minhaLista.Where(l => l.UsuarioId == _usuarioAtualId);

        // =========================
        // PERSISTÊNCIA (JSON)
        // =========================
        public void CarregarDados()
        {
            lock (_lock)
            {
                var dados = _storage.Carregar();

                _usuarios.Clear();
                _usuarios.AddRange(dados.Usuarios);

                if (dados.UsuarioAtualId != Guid.Empty)
                    _usuarioAtualId = dados.UsuarioAtualId;

                _catalogo.Clear();
                _catalogo.AddRange(dados.Catalogo);

                _minhaLista.Clear();
                _minhaLista.AddRange(dados.MinhaLista);

                // Migração simples: leituras antigas sem UsuarioId
                bool migrou = false;
                for (int i = 0; i < _minhaLista.Count; i++)
                {
                    if (_minhaLista[i].UsuarioId == Guid.Empty && _usuarioAtualId != Guid.Empty)
                    {
                        _minhaLista[i].UsuarioId = _usuarioAtualId;
                        migrou = true;
                    }
                }

                if (migrou) AutoSalvar();
            }
        }

        public void SalvarDados()
        {
            lock (_lock)
            {
                var dados = new DadosApp
                {
                    Catalogo = _catalogo.ToList(),
                    Usuarios = _usuarios.ToList(),
                    MinhaLista = _minhaLista.ToList(),
                    UsuarioAtualId = _usuarioAtualId
                };
                _storage.Salvar(dados);
            }
        }

        public string CaminhoDoArquivoDeDados() => _storage.CaminhoArquivo;

        // =========================
        // CADASTRO / LOGIN
        // =========================
        public void CadastrarNovoUsuario(string nome, string email, string senha)
        {
            lock (_lock)
            {
                if (_usuarios.Any(u => u.Nome.Equals(nome, StringComparison.OrdinalIgnoreCase)))
                    throw new Exception("Este nome de usuário já está sendo usado.");

                if (_usuarios.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
                    throw new Exception("Este e-mail já está cadastrado em outra conta.");

                var novo = new Usuario
                {
                    Id = Guid.NewGuid(),
                    Nome = nome,
                    Email = email,
                    Senha = senha,
                    EhAdmin = false
                };

                _usuarios.Add(novo);
                AutoSalvar();
            }
        }

        public Usuario ValidarLogin(string identificador, string senha)
        {
            lock (_lock)
            {
                var usuario = _usuarios.FirstOrDefault(u =>
                    u.Nome.Equals(identificador, StringComparison.OrdinalIgnoreCase) ||
                    u.Email.Equals(identificador, StringComparison.OrdinalIgnoreCase));

                if (usuario == null || usuario.Senha != senha)
                    throw new Exception("Usuário/E-mail ou senha incorretos.");

                _usuarioAtualId = usuario.Id;
                AutoSalvar();

                return usuario;
            }
        }

        // =========================
        // HELPERS
        // =========================
        private static string NormalizarTitulo(string titulo)
            => titulo.Trim().ToUpperInvariant();

        private static string? NormalizeEditoraKey(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            return s.Trim().ToLowerInvariant();
        }
    }
}
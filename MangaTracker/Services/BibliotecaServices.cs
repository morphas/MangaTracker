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
            // Agora ele busca o dados.json na mesma pasta onde seu código está aberto
            string arquivo = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "dados.json");
            _storage = new JsonStorage(arquivo);
        }

        private void AutoSalvar()
        {
            SalvarDados();
        }

        // Método que o seu Controller usa para checar se você é Admin
        public Usuario? ObterUsuarioLogado()
        {
            lock (_lock)
            {
                return _usuarios.FirstOrDefault(u => u.Id == _usuarioAtualId);
            }
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

        public Manga CadastrarNoCatalogo(string titulo, bool lancadoNoBrasil, string? editora)
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
                Editora = editora
            };

            _catalogo.Add(manga);
            SalvarDados();
            return manga;
        }

        public void DefinirTotalCapitulos(Guid mangaId, int? totalCapitulos)
        {
            Manga? manga = BuscarMangaPorId(mangaId);
            if (manga is null) return;

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

            leitura.CapituloAtual = capituloAtual < 0 ? 0 : capituloAtual;
            leitura.UltimaLeituraEm = DateTime.Now;

            if (status.HasValue)
                leitura.Status = status.Value;

            var manga = BuscarMangaPorId(mangaId);
            if (manga?.TotalCapitulos is not null && leitura.CapituloAtual >= manga.TotalCapitulos.Value)
            {
                leitura.Status = StatusLeitura.Concluido;
                leitura.CapituloAtual = manga.TotalCapitulos.Value;
            }
            AutoSalvar();
        }

        public bool EstaNaMinhaLista(Guid mangaId)
            => LeiturasDoUsuarioAtual().Any(l => l.MangaId == mangaId);

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

                if (_usuarios.Count == 0)
                {
                    // Removemos a criação do "Rafael". Agora o sistema depende do seu dados.json.
                    SalvarDados();
                }

                _catalogo.Clear();
                _catalogo.AddRange(dados.Catalogo);

                _minhaLista.Clear();
                _minhaLista.AddRange(dados.MinhaLista);

                bool migrou = false;
                for (int i = 0; i < _minhaLista.Count; i++)
                {
                    if (_minhaLista[i].UsuarioId == Guid.Empty)
                    {
                        _minhaLista[i].UsuarioId = _usuarioAtualId;
                        migrou = true;
                    }
                }

                if (migrou) SalvarDados();
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
                    MinhaLista = _minhaLista.ToList(), // Corrigido: Agora salva suas leituras
                    UsuarioAtualId = _usuarioAtualId   // Corrigido: Salva quem está logado
                };
                _storage.Salvar(dados);
            }
        }

        // =========================
        // USUÁRIOS E HELPERS
        // =========================
        public IReadOnlyList<Usuario> ListarUsuarios() => _usuarios;

        public Usuario? BuscarUsuarioPorId(Guid id)
            => _usuarios.FirstOrDefault(u => u.Id == id);

        public bool DefinirUsuarioAtual(Guid usuarioId)
        {
            if (BuscarUsuarioPorId(usuarioId) is null) return false;
            _usuarioAtualId = usuarioId;
            AutoSalvar();
            return true;
        }

        private IEnumerable<Leitura> LeiturasDoUsuarioAtual()
            => _minhaLista.Where(l => l.UsuarioId == _usuarioAtualId);

        private static string NormalizarTitulo(string titulo)
            => titulo.Trim().ToUpperInvariant();

        public string CaminhoDoArquivoDeDados() => _storage.CaminhoArquivo;

        public void CadastrarNovoUsuario(string nome, string email, string senha)
        {
            // 1. Verificamos se o nome já existe (você já tinha essa parte)
            if (_usuarios.Any(u => u.Nome.Equals(nome, StringComparison.OrdinalIgnoreCase)))
            {
                throw new Exception("Este nome de usuário já está sendo usado.");
            }

            // 2. NOVA REGRA: Verificamos se o e-mail já existe
            // O 'Any' vai percorrer a lista procurando alguém com o mesmo e-mail
            if (_usuarios.Any(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
            {
                throw new Exception("Este e-mail já está cadastrado em outra conta.");
            }

            // 3. Se passou pelas duas travas, criamos o usuário
            var novo = new Usuario
            {
                Id = Guid.NewGuid(),
                Nome = nome,
                Email = email,
                Senha = senha,
                EhAdmin = false
            };

            _usuarios.Add(novo);
            SalvarDados();
        }

        public Usuario ValidarLogin(string identificador, string senha)
        {
            // Procura na lista de usuários por Nome OU por Email
            var usuario = _usuarios.FirstOrDefault(u =>
                u.Nome.Equals(identificador, StringComparison.OrdinalIgnoreCase) ||
                u.Email.Equals(identificador, StringComparison.OrdinalIgnoreCase));

            // Se não achar ninguém ou a senha estiver errada, avisa o erro
            if (usuario == null || usuario.Senha != senha)
            {
                throw new Exception("Usuário/E-mail ou senha incorretos.");
            }

            // Se deu certo, ele vira o usuário ativo do sistema
            _usuarioAtualId = usuario.Id;
            AutoSalvar();

            return usuario;
        }
    }
}
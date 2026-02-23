using MangaTracker.Models;
using MangaTracker.Services;
using MangaTracker.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace MangaTracker.Api.Services
{
    public class BibliotecaServiceDb : IBibliotecaService
    {
        private readonly MangaTrackerDbContext _db;
        private Guid _usuarioAtualId = Guid.Empty;

        public BibliotecaServiceDb(MangaTrackerDbContext db)
        {
            _db = db;
        }

        public void CarregarDados()
        {
            // No banco não precisa carregar JSON
            // Opcional: criar admin inicial se não existir
            if (!_db.Usuarios.Any())
            {
                _db.Usuarios.Add(new Usuario
                {
                    Nome = "admin",
                    Email = "admin@mangatracker.com",
                    Senha = "admin",
                    EhAdmin = true
                });
                _db.SaveChanges();
            }
        }

        public void SalvarDados() { /* não usado no banco */ }

        public string CaminhoDoArquivoDeDados() => "Postgres";

        public Usuario? ObterUsuarioLogado()
        {
            if (_usuarioAtualId == Guid.Empty) return null;
            return _db.Usuarios.FirstOrDefault(u => u.Id == _usuarioAtualId);
        }

        public Usuario ValidarLogin(string identificador, string senha)
        {
            var usuario = _db.Usuarios.FirstOrDefault(u =>
                u.Nome.ToLower() == identificador.ToLower() ||
                u.Email.ToLower() == identificador.ToLower());

            if (usuario == null || usuario.Senha != senha)
                throw new Exception("Usuário/E-mail ou senha incorretos.");

            _usuarioAtualId = usuario.Id;
            return usuario;
        }

        public void CadastrarNovoUsuario(string nome, string email, string senha)
        {
            if (_db.Usuarios.Any(u => u.Nome.ToLower() == nome.ToLower()))
                throw new Exception("Este nome de usuário já está sendo usado.");

            if (_db.Usuarios.Any(u => u.Email.ToLower() == email.ToLower()))
                throw new Exception("Este e-mail já está cadastrado em outra conta.");

            _db.Usuarios.Add(new Usuario
            {
                Nome = nome,
                Email = email,
                Senha = senha,
                EhAdmin = false
            });
            _db.SaveChanges();
        }

        public IReadOnlyList<Usuario> ListarUsuarios() => _db.Usuarios.AsNoTracking().ToList();

        public bool DefinirUsuarioAtual(Guid usuarioId)
        {
            if (!_db.Usuarios.Any(u => u.Id == usuarioId)) return false;
            _usuarioAtualId = usuarioId;
            return true;
        }

        public IReadOnlyList<Manga> ListarCatalogo() => _db.Catalogo.AsNoTracking().ToList();

        public Manga? BuscarMangaPorId(Guid id) => _db.Catalogo.FirstOrDefault(m => m.Id == id);

        public Manga? BuscarMangaPorTitulo(string titulo)
        {
            var t = titulo.Trim().ToUpperInvariant();
            return _db.Catalogo.FirstOrDefault(m => m.Titulo.ToUpper() == t);
        }

        public bool MangaExisteNoCatalogo(string titulo) => BuscarMangaPorTitulo(titulo) != null;

        private static string? NormalizeEditoraKey(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            return s.Trim().ToLowerInvariant();
        }

        public Manga CadastrarNoCatalogo(string titulo, bool lancadoNoBrasil, string? editora)
        {
            var t = titulo.Trim();

            if (MangaExisteNoCatalogo(t))
                throw new InvalidOperationException("Esse mangá já existe no catálogo.");

            if (!lancadoNoBrasil)
                editora = null;

            var manga = new Manga
            {
                Titulo = t,
                LancadoNoBrasil = lancadoNoBrasil,
                Editora = string.IsNullOrWhiteSpace(editora) ? null : editora.Trim()
            };

            _db.Catalogo.Add(manga);
            _db.SaveChanges();
            return manga;
        }

        public void DefinirTotalCapitulos(Guid mangaId, int? totalCapitulos)
        {
            var manga = _db.Catalogo.FirstOrDefault(m => m.Id == mangaId);
            if (manga == null) return;
            manga.TotalCapitulos = totalCapitulos;
            _db.SaveChanges();
        }

        public IReadOnlyList<(Manga Manga, Leitura Leitura)> ListarMinhaLista()
        {
            if (_usuarioAtualId == Guid.Empty) return new List<(Manga, Leitura)>();

            var leituras = _db.Leituras.Where(l => l.UsuarioId == _usuarioAtualId).ToList();
            var catalogo = _db.Catalogo.ToDictionary(m => m.Id, m => m);

            return leituras
                .Where(l => catalogo.ContainsKey(l.MangaId))
                .Select(l => (catalogo[l.MangaId], l))
                .ToList();
        }

        public bool EstaNaMinhaLista(Guid mangaId)
            => _usuarioAtualId != Guid.Empty && _db.Leituras.Any(l => l.UsuarioId == _usuarioAtualId && l.MangaId == mangaId);

        public void AdicionarNaMinhaLista(Guid mangaId, StatusLeitura status, int? capituloAtual = null)
        {
            if (_usuarioAtualId == Guid.Empty) throw new Exception("Faça login primeiro.");
            if (!_db.Catalogo.Any(m => m.Id == mangaId)) throw new Exception("Mangá não encontrado no catálogo.");
            if (EstaNaMinhaLista(mangaId)) throw new Exception("Esse mangá já está na sua lista de leitura.");

            var leitura = new Leitura
            {
                UsuarioId = _usuarioAtualId,
                MangaId = mangaId,
                Status = status,
                CapituloAtual = capituloAtual ?? 0,
                UltimaLeituraEm = DateTime.UtcNow
            };

            _db.Leituras.Add(leitura);
            _db.SaveChanges();
        }

        public void AtualizarLeitura(Guid mangaId, int capituloAtual, StatusLeitura? status = null)
        {
            if (_usuarioAtualId == Guid.Empty) throw new Exception("Faça login primeiro.");

            var leitura = _db.Leituras.FirstOrDefault(l => l.UsuarioId == _usuarioAtualId && l.MangaId == mangaId);
            if (leitura == null) return;

            leitura.CapituloAtual = Math.Max(0, capituloAtual);
            leitura.UltimaLeituraEm = DateTime.UtcNow;
            if (status.HasValue) leitura.Status = status.Value;

            _db.SaveChanges();
        }
    }
}
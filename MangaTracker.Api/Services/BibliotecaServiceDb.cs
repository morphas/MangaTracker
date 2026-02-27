using System;
using System.Collections.Generic;
using System.Linq;
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
            if (string.IsNullOrWhiteSpace(identificador))
                throw new Exception("Usuário/E-mail é obrigatório.");

            var id = identificador.Trim().ToLowerInvariant();

            var usuario = _db.Usuarios.FirstOrDefault(u =>
                u.Nome.ToLower() == id ||
                u.Email.ToLower() == id);

            if (usuario == null || usuario.Senha != senha)
                throw new Exception("Usuário/E-mail ou senha incorretos.");

            _usuarioAtualId = usuario.Id;
            return usuario;
        }

        public void CadastrarNovoUsuario(string nome, string email, string senha)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new Exception("Nome é obrigatório.");

            if (string.IsNullOrWhiteSpace(email))
                throw new Exception("E-mail é obrigatório.");

            if (string.IsNullOrWhiteSpace(senha))
                throw new Exception("Senha é obrigatória.");

            var n = nome.Trim().ToLowerInvariant();
            var e = email.Trim().ToLowerInvariant();

            if (_db.Usuarios.Any(u => u.Nome.ToLower() == n))
                throw new Exception("Este nome de usuário já está sendo usado.");

            if (_db.Usuarios.Any(u => u.Email.ToLower() == e))
                throw new Exception("Este e-mail já está cadastrado em outra conta.");

            _db.Usuarios.Add(new Usuario
            {
                Nome = nome.Trim(),
                Email = email.Trim(),
                Senha = senha,
                EhAdmin = false
            });

            _db.SaveChanges();
        }

        public IReadOnlyList<Usuario> ListarUsuarios()
            => _db.Usuarios.AsNoTracking().ToList();

        public bool DefinirUsuarioAtual(Guid usuarioId)
        {
            if (!_db.Usuarios.Any(u => u.Id == usuarioId)) return false;
            _usuarioAtualId = usuarioId;
            return true;
        }

        // =========================
        // LOGS DE ADMIN
        // =========================
        private void LogAdmin(string acao, string? detalhes = null)
        {
            if (_usuarioAtualId == Guid.Empty) return;

            var u = _db.Usuarios.AsNoTracking().FirstOrDefault(x => x.Id == _usuarioAtualId);
            if (u is null || !u.EhAdmin) return;

            _db.AdminLogs.Add(new AdminLog
            {
                AdminId = _usuarioAtualId,
                Acao = acao,
                Detalhes = detalhes,
                CriadoEm = DateTime.UtcNow
            });
        }

        // =========================
        // CATÁLOGO
        // =========================

        public IReadOnlyList<Manga> ListarCatalogo()
            => _db.Catalogo.AsNoTracking().ToList();

        public Manga? BuscarMangaPorId(Guid id)
            => _db.Catalogo.FirstOrDefault(m => m.Id == id);

        public Manga? BuscarMangaPorTitulo(string titulo)
        {
            var t = (titulo ?? "").Trim().ToLowerInvariant();
            return _db.Catalogo.FirstOrDefault(m => (m.Titulo ?? "").ToLower() == t);
        }

        public bool MangaExisteNoCatalogo(string titulo)
            => BuscarMangaPorTitulo(titulo) != null;

        private static string? NormalizeEditoraKey(string? s)
        {
            if (string.IsNullOrWhiteSpace(s)) return null;
            return s.Trim().ToLowerInvariant();
        }

        public Manga CadastrarNoCatalogo(string titulo, bool lancadoNoBrasil, string? editora)
        {
            var t = (titulo ?? "").Trim();
            if (string.IsNullOrWhiteSpace(t))
                throw new Exception("Título é obrigatório.");

            if (lancadoNoBrasil && string.IsNullOrWhiteSpace(editora))
                throw new Exception("Editora é obrigatória quando o mangá é lançado no Brasil.");

            if (!lancadoNoBrasil)
                editora = null;

            if (MangaExisteNoCatalogo(t))
                throw new InvalidOperationException("Esse mangá já existe no catálogo.");

            var editoraFinal = string.IsNullOrWhiteSpace(editora) ? null : editora.Trim();

            var manga = new Manga
            {
                Titulo = t,
                LancadoNoBrasil = lancadoNoBrasil,
                Editora = editoraFinal,
                EditoraKey = NormalizeEditoraKey(editoraFinal)
            };

            _db.Catalogo.Add(manga);

            LogAdmin("CATALOGO_CRIAR", $"Criou: {manga.Titulo} | BR={manga.LancadoNoBrasil} | Editora={manga.Editora ?? "—"}");

            _db.SaveChanges();
            return manga;
        }

        public Manga AtualizarMangaDoCatalogo(Guid mangaId, string titulo, bool lancadoNoBrasil, string? editora)
        {
            if (mangaId == Guid.Empty)
                throw new Exception("ID inválido.");

            var manga = _db.Catalogo.FirstOrDefault(m => m.Id == mangaId);
            if (manga is null)
                throw new Exception("Mangá não encontrado.");

            var t = (titulo ?? "").Trim();
            if (string.IsNullOrWhiteSpace(t))
                throw new Exception("Título é obrigatório.");

            var existeOutro = _db.Catalogo.Any(m => m.Id != mangaId && m.Titulo.ToLower() == t.ToLower());
            if (existeOutro)
                throw new Exception("Já existe outro mangá com esse título no catálogo.");

            if (lancadoNoBrasil && string.IsNullOrWhiteSpace(editora))
                throw new Exception("Informe a editora se o mangá for lançado no Brasil.");

            if (!lancadoNoBrasil)
                editora = null;

            var antes = $"{manga.Titulo} | BR={manga.LancadoNoBrasil} | Editora={manga.Editora ?? "—"}";

            manga.Titulo = t;
            manga.LancadoNoBrasil = lancadoNoBrasil;

            var editoraFinal = string.IsNullOrWhiteSpace(editora) ? null : editora.Trim();
            manga.Editora = editoraFinal;
            manga.EditoraKey = NormalizeEditoraKey(editoraFinal);

            var depois = $"{manga.Titulo} | BR={manga.LancadoNoBrasil} | Editora={manga.Editora ?? "—"}";
            LogAdmin("CATALOGO_EDITAR", $"ID={manga.Id} | Antes: {antes} | Depois: {depois}");

            _db.SaveChanges();
            return manga;
        }

        public Manga AtualizarDetalhesManga(
    Guid mangaId,
    string? capaUrl,
    string? descricao,
    string? demografia,
    string? autor,
    int? anoLancamentoOriginal,
    int? anoLancamentoBrasil
)
        {
            if (mangaId == Guid.Empty)
                throw new Exception("ID inválido.");

            var manga = _db.Catalogo.FirstOrDefault(m => m.Id == mangaId);
            if (manga is null)
                throw new Exception("Mangá não encontrado.");

            // Normaliza strings (evita salvar " " no banco)
            static string? Clean(string? s)
            {
                if (string.IsNullOrWhiteSpace(s)) return null;
                return s.Trim();
            }

            var antes =
                $"Capa={(manga.CapaUrl ?? "—")} | " +
                $"Demo={(manga.Demografia ?? "—")} | " +
                $"Autor={(manga.Autor ?? "—")} | " +
                $"AnoOrig={(manga.AnoLancamentoOriginal?.ToString() ?? "—")} | " +
                $"AnoBR={(manga.AnoLancamentoBrasil?.ToString() ?? "—")}";

            manga.CapaUrl = Clean(capaUrl);
            manga.Descricao = Clean(descricao);
            manga.Demografia = Clean(demografia);
            manga.Autor = Clean(autor);
            manga.AnoLancamentoOriginal = anoLancamentoOriginal;
            manga.AnoLancamentoBrasil = anoLancamentoBrasil;

            var depois =
                $"Capa={(manga.CapaUrl ?? "—")} | " +
                $"Demo={(manga.Demografia ?? "—")} | " +
                $"Autor={(manga.Autor ?? "—")} | " +
                $"AnoOrig={(manga.AnoLancamentoOriginal?.ToString() ?? "—")} | " +
                $"AnoBR={(manga.AnoLancamentoBrasil?.ToString() ?? "—")}";

            // ✅ LOG (admin)
            LogAdmin("CATALOGO_DETALHES", $"ID={manga.Id} | {manga.Titulo} | Antes: {antes} | Depois: {depois}");

            _db.SaveChanges();
            return manga;
        }

        // ✅ NOVO: atualizar detalhes (admin preenche depois)
        

        public void RemoverMangaDoCatalogo(Guid mangaId)
        {
            if (mangaId == Guid.Empty)
                throw new Exception("ID inválido.");

            var manga = _db.Catalogo.FirstOrDefault(m => m.Id == mangaId);
            if (manga is null)
                throw new Exception("Mangá não encontrado.");

            LogAdmin("CATALOGO_EXCLUIR", $"Excluiu: {manga.Titulo} | ID={manga.Id}");

            var leituras = _db.Leituras.Where(l => l.MangaId == mangaId).ToList();
            if (leituras.Count > 0)
                _db.Leituras.RemoveRange(leituras);

            _db.Catalogo.Remove(manga);
            _db.SaveChanges();
        }

        public void DefinirTotalCapitulos(Guid mangaId, int? totalCapitulos)
        {
            var manga = _db.Catalogo.FirstOrDefault(m => m.Id == mangaId);
            if (manga == null) return;
            manga.TotalCapitulos = totalCapitulos;
            _db.SaveChanges();
        }

        // =========================
        // MINHA LISTA (LEITURA)
        // =========================

        public IReadOnlyList<(Manga Manga, Leitura Leitura)> ListarMinhaLista()
        {
            if (_usuarioAtualId == Guid.Empty) return new List<(Manga, Leitura)>();

            var leituras = _db.Leituras.AsNoTracking()
                .Where(l => l.UsuarioId == _usuarioAtualId)
                .ToList();

            var catalogo = _db.Catalogo.AsNoTracking()
                .ToDictionary(m => m.Id, m => m);

            return leituras
                .Where(l => catalogo.ContainsKey(l.MangaId))
                .Select(l => (catalogo[l.MangaId], l))
                .ToList();
        }

        public bool EstaNaMinhaLista(Guid mangaId)
            => _usuarioAtualId != Guid.Empty &&
               _db.Leituras.Any(l => l.UsuarioId == _usuarioAtualId && l.MangaId == mangaId);

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

        public void RemoverDaMinhaLista(Guid mangaId)
        {
            if (_usuarioAtualId == Guid.Empty)
                throw new Exception("Faça login primeiro.");

            var leitura = _db.Leituras.FirstOrDefault(l =>
                l.UsuarioId == _usuarioAtualId && l.MangaId == mangaId);

            if (leitura == null)
                throw new Exception("Esse mangá não está na sua lista.");

            _db.Leituras.Remove(leitura);
            _db.SaveChanges();
        }
    }
}
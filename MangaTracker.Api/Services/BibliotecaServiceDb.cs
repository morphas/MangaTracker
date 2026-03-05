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

        // =========================
        // BOOT
        // =========================
        public void CarregarDados()
        {
            // Cria admin inicial se não existir nenhum usuário.
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

        // =========================
        // USUÁRIOS
        // =========================
        public Usuario? ObterUsuarioLogado()
        {
            if (_usuarioAtualId == Guid.Empty) return null;
            return _db.Usuarios.FirstOrDefault(u => u.Id == _usuarioAtualId);
        }

        public IReadOnlyList<Usuario> ListarUsuarios()
            => _db.Usuarios.AsNoTracking().ToList();

        public bool DefinirUsuarioAtual(Guid usuarioId)
        {
            if (!_db.Usuarios.Any(u => u.Id == usuarioId)) return false;
            _usuarioAtualId = usuarioId;
            return true;
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
        // EDITORAS
        // =========================
        private static string NormalizeKey(string s)
        {
            return (s ?? "").Trim().ToLowerInvariant();
        }

        public IReadOnlyList<Editora> ListarEditoras()
            => _db.Editoras.AsNoTracking()
                .OrderBy(e => e.Nome)
                .ToList();

        public Editora? BuscarEditoraPorId(Guid id)
            => _db.Editoras.FirstOrDefault(e => e.Id == id);

        public Editora CriarEditora(string nome, string? descricao)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new Exception("Nome é obrigatório.");

            var nomeFinal = nome.Trim();
            var key = NormalizeKey(nomeFinal);

            if (_db.Editoras.Any(e => e.Key == key))
                throw new Exception("Já existe uma editora com esse nome (key duplicada).");

            var editora = new Editora
            {
                Nome = nomeFinal,
                Key = key,
                Descricao = string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim(),
                CriadoEm = DateTime.UtcNow
            };

            _db.Editoras.Add(editora);
            LogAdmin("EDITORA_CRIAR", $"Criou editora: {editora.Nome} (key={editora.Key})");
            _db.SaveChanges();

            return editora;
        }

        public Editora AtualizarEditora(Guid id, string nome, string? descricao)
        {
            if (id == Guid.Empty) throw new Exception("ID inválido.");
            if (string.IsNullOrWhiteSpace(nome)) throw new Exception("Nome é obrigatório.");

            var editora = _db.Editoras.FirstOrDefault(e => e.Id == id);
            if (editora is null) throw new Exception("Editora não encontrada.");

            var nomeFinal = nome.Trim();
            var key = NormalizeKey(nomeFinal);

            var existeOutra = _db.Editoras.Any(e => e.Id != id && e.Key == key);
            if (existeOutra)
                throw new Exception("Já existe outra editora com esse nome (key duplicada).");

            var antes = $"{editora.Nome} (key={editora.Key})";

            editora.Nome = nomeFinal;
            editora.Key = key;
            editora.Descricao = string.IsNullOrWhiteSpace(descricao) ? null : descricao.Trim();

            var depois = $"{editora.Nome} (key={editora.Key})";
            LogAdmin("EDITORA_EDITAR", $"ID={editora.Id} | Antes: {antes} | Depois: {depois}");

            _db.SaveChanges();
            return editora;
        }

        public void RemoverEditora(Guid id)
        {
            if (id == Guid.Empty) throw new Exception("ID inválido.");

            var editora = _db.Editoras.FirstOrDefault(e => e.Id == id);
            if (editora is null) throw new Exception("Editora não encontrada.");

            var temMangaVinculado = _db.Catalogo.Any(m => m.EditoraId == id);
            if (temMangaVinculado)
                throw new Exception("Não é possível excluir: existe(m) mangá(s) vinculado(s) a esta editora.");

            LogAdmin("EDITORA_EXCLUIR", $"Excluiu editora: {editora.Nome} (ID={editora.Id})");

            _db.Editoras.Remove(editora);
            _db.SaveChanges();
        }

        // =========================
        // CATÁLOGO
        // =========================
        public IReadOnlyList<Manga> ListarCatalogo()
            => _db.Catalogo
                .AsNoTracking()
                .Include(m => m.EditoraNav)
                .ToList();

        public PagedResult<Manga> ListarCatalogoPaginado(bool? lancadoNoBrasil, Guid? editoraId, string? q, int page, int pageSize)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 200) pageSize = 200;

            var query = _db.Catalogo
                .AsNoTracking()
                .Include(m => m.EditoraNav)
                .AsQueryable();

            if (lancadoNoBrasil.HasValue)
                query = query.Where(m => m.LancadoNoBrasil == lancadoNoBrasil.Value);

            if (editoraId.HasValue && editoraId.Value != Guid.Empty)
                query = query.Where(m => m.EditoraId == editoraId.Value);

            if (!string.IsNullOrWhiteSpace(q))
            {
                var termo = q.Trim().ToLowerInvariant();
                query = query.Where(m => (m.Titulo ?? "").ToLower().Contains(termo));
            }

            var total = query.Count();

            var items = query
                .OrderBy(m => m.Titulo)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new PagedResult<Manga>(items, total, page, pageSize);
        }

        public Manga? BuscarMangaPorId(Guid id)
            => _db.Catalogo
                .Include(m => m.EditoraNav)
                .FirstOrDefault(m => m.Id == id);

        public Manga? BuscarMangaPorTitulo(string titulo)
        {
            var t = (titulo ?? "").Trim().ToLowerInvariant();
            return _db.Catalogo.FirstOrDefault(m => (m.Titulo ?? "").ToLower() == t);
        }

        public bool MangaExisteNoCatalogo(string titulo)
            => BuscarMangaPorTitulo(titulo) != null;

        public Manga CadastrarNoCatalogo(string titulo, bool lancadoNoBrasil, Guid? editoraId)
        {
            var t = (titulo ?? "").Trim();
            if (string.IsNullOrWhiteSpace(t))
                throw new Exception("Título é obrigatório.");

            if (MangaExisteNoCatalogo(t))
                throw new InvalidOperationException("Esse mangá já existe no catálogo.");

            if (lancadoNoBrasil)
            {
                if (!editoraId.HasValue || editoraId.Value == Guid.Empty)
                    throw new Exception("Informe a editora se o mangá for lançado no Brasil.");

                var ed = _db.Editoras.AsNoTracking().FirstOrDefault(e => e.Id == editoraId.Value);
                if (ed is null)
                    throw new Exception("Editora inválida (não encontrada).");
            }
            else
            {
                editoraId = null;
            }

            var manga = new Manga
            {
                Titulo = t,
                LancadoNoBrasil = lancadoNoBrasil,
                EditoraId = editoraId,
                CriadoEm = DateTime.UtcNow,
                Generos = new List<string>() // ✅ garante não-null
            };

            _db.Catalogo.Add(manga);

            LogAdmin("CATALOGO_CRIAR", $"Criou: {manga.Titulo} | BR={manga.LancadoNoBrasil} | EditoraId={manga.EditoraId?.ToString() ?? "—"}");

            _db.SaveChanges();

            return BuscarMangaPorId(manga.Id)!;
        }

        public Manga AtualizarMangaDoCatalogo(Guid mangaId, string titulo, bool lancadoNoBrasil, Guid? editoraId)
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

            if (lancadoNoBrasil)
            {
                if (!editoraId.HasValue || editoraId.Value == Guid.Empty)
                    throw new Exception("Informe a editora se o mangá for lançado no Brasil.");

                var ed = _db.Editoras.AsNoTracking().FirstOrDefault(e => e.Id == editoraId.Value);
                if (ed is null)
                    throw new Exception("Editora inválida (não encontrada).");
            }
            else
            {
                editoraId = null;
            }

            var antes = $"{manga.Titulo} | BR={manga.LancadoNoBrasil} | EditoraId={manga.EditoraId?.ToString() ?? "—"}";

            manga.Titulo = t;
            manga.LancadoNoBrasil = lancadoNoBrasil;
            manga.EditoraId = editoraId;

            var depois = $"{manga.Titulo} | BR={manga.LancadoNoBrasil} | EditoraId={manga.EditoraId?.ToString() ?? "—"}";
            LogAdmin("CATALOGO_EDITAR", $"ID={manga.Id} | Antes: {antes} | Depois: {depois}");

            _db.SaveChanges();

            return BuscarMangaPorId(manga.Id)!;
        }

        // ✅ Detalhes avançados (com generos list)
        public Manga AtualizarDetalhesManga(
            Guid mangaId,
            string? capaUrl,
            string? descricao,
            string? demografia,
            string? autor,
            int? anoLancamentoOriginal,
            int? anoLancamentoBrasil,
            List<string>? generos
        )
        {
            if (mangaId == Guid.Empty)
                throw new Exception("ID inválido.");

            var manga = _db.Catalogo.FirstOrDefault(m => m.Id == mangaId);
            if (manga is null)
                throw new Exception("Mangá não encontrado.");

            static string? Clean(string? s)
            {
                if (string.IsNullOrWhiteSpace(s)) return null;
                return s.Trim();
            }

            // normaliza e limita 4 também no backend
            List<string> NormalizeList(List<string>? list)
            {
                if (list is null) return new List<string>();
                return list
                    .Select(x => (x ?? "").Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Take(4)
                    .ToList();
            }

            var antes =
                $"Capa={(manga.CapaUrl ?? "—")} | " +
                $"Demo={(manga.Demografia ?? "—")} | " +
                $"Autor={(manga.Autor ?? "—")} | " +
                $"AnoOrig={(manga.AnoLancamentoOriginal?.ToString() ?? "—")} | " +
                $"AnoBR={(manga.AnoLancamentoBrasil?.ToString() ?? "—")} | " +
                $"Generos={((manga.Generos != null && manga.Generos.Count > 0) ? string.Join(",", manga.Generos) : "—")}";

            manga.CapaUrl = Clean(capaUrl);
            manga.Descricao = Clean(descricao);
            manga.Demografia = Clean(demografia);
            manga.Autor = Clean(autor);
            manga.AnoLancamentoOriginal = anoLancamentoOriginal;
            manga.AnoLancamentoBrasil = anoLancamentoBrasil;

            // ✅ aqui salva a lista no Postgres text[]
            manga.Generos = NormalizeList(generos);

            var depois =
                $"Capa={(manga.CapaUrl ?? "—")} | " +
                $"Demo={(manga.Demografia ?? "—")} | " +
                $"Autor={(manga.Autor ?? "—")} | " +
                $"AnoOrig={(manga.AnoLancamentoOriginal?.ToString() ?? "—")} | " +
                $"AnoBR={(manga.AnoLancamentoBrasil?.ToString() ?? "—")} | " +
                $"Generos={((manga.Generos != null && manga.Generos.Count > 0) ? string.Join(",", manga.Generos) : "—")}";

            LogAdmin("CATALOGO_DETALHES", $"ID={manga.Id} | {manga.Titulo} | Antes: {antes} | Depois: {depois}");

            _db.SaveChanges();

            return BuscarMangaPorId(manga.Id)!;
        }

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

            manga.TotalCapitulos = totalCapitulos ?? 0;
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
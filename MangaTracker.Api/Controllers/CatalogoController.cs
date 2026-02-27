using Microsoft.AspNetCore.Mvc;
using MangaTracker.Models;
using MangaTracker.Services;

namespace MangaTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogoController : ControllerBase
    {
        private readonly IBibliotecaService _service;

        public CatalogoController(IBibliotecaService service)
        {
            _service = service;
        }

        // DTOs (cadastro/edição rápida)
        public record CadastrarMangaDto(string Titulo, bool LancadoNoBrasil, string? Editora);
        public record AtualizarMangaDto(string Titulo, bool LancadoNoBrasil, string? Editora);

        // DTO (detalhes avançados - admin)
        public record AtualizarMangaDetalhesDto(
            string? CapaUrl,
            string? Descricao,
            string? Demografia,
            string? Autor,
            int? AnoLancamentoOriginal,
            int? AnoLancamentoBrasil
        );

        // ========= Helpers =========
        private bool TrySetAdmin(Guid? userId, out IActionResult? erro)
        {
            erro = null;

            if (userId is null)
            {
                erro = Unauthorized(new { erro = "Usuário não informado no header X-User-Id." });
                return false;
            }

            if (!_service.DefinirUsuarioAtual(userId.Value))
            {
                erro = Unauthorized(new { erro = "Usuário não encontrado." });
                return false;
            }

            var usuarioLogado = _service.ObterUsuarioLogado();
            if (usuarioLogado is null)
            {
                erro = Unauthorized(new { erro = "Faça login como admin." });
                return false;
            }

            if (!usuarioLogado.EhAdmin)
            {
                erro = StatusCode(403, new { erro = "Apenas administradores podem executar esta ação." });
                return false;
            }

            return true;
        }

        private static bool UrlValida(string? url)
        {
            if (string.IsNullOrWhiteSpace(url)) return true; // opcional
            return Uri.TryCreate(url.Trim(), UriKind.Absolute, out var u)
                   && (u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps);
        }

        private static bool AnoValido(int? ano)
        {
            if (!ano.HasValue) return true; // opcional
            var a = ano.Value;
            var max = DateTime.Now.Year + 1; // tolera "ano que vem"
            return a >= 1900 && a <= max;
        }

        // =========================
        // GET: api/catalogo
        // Filtros:
        // ?lancadoNoBrasil=true|false
        // ?editora=panini
        // ?q=jujutsu
        // =========================
        [HttpGet]
        public ActionResult<IEnumerable<Manga>> Get(
            [FromQuery] bool? lancadoNoBrasil,
            [FromQuery] string? editora,
            [FromQuery] string? q)
        {
            var lista = _service.ListarCatalogo().AsQueryable();

            if (lancadoNoBrasil.HasValue)
                lista = lista.Where(m => m.LancadoNoBrasil == lancadoNoBrasil.Value);

            if (!string.IsNullOrWhiteSpace(editora))
            {
                var key = editora.Trim().ToLowerInvariant();
                lista = lista.Where(m => m.EditoraKey == key);
            }

            if (!string.IsNullOrWhiteSpace(q))
            {
                var termo = q.Trim().ToLowerInvariant();
                lista = lista.Where(m => (m.Titulo ?? "").ToLower().Contains(termo));
            }

            return Ok(
                lista
                    .OrderBy(m => m.Titulo, StringComparer.CurrentCultureIgnoreCase)
                    .ToList()
            );
        }

        // =========================
        // GET: api/catalogo/{id}
        // =========================
        [HttpGet("{id:guid}")]
        public ActionResult<Manga> GetById(Guid id)
        {
            var manga = _service.BuscarMangaPorId(id);
            if (manga is null)
                return NotFound(new { erro = "Mangá não encontrado." });

            return Ok(manga);
        }

        // =========================
        // POST: api/catalogo (admin)
        // =========================
        [HttpPost]
        public IActionResult Post(
            [FromHeader(Name = "X-User-Id")] Guid? userId,
            [FromBody] CadastrarMangaDto dto)
        {
            if (!TrySetAdmin(userId, out var erro))
                return erro!;

            if (dto is null || string.IsNullOrWhiteSpace(dto.Titulo))
                return BadRequest(new { erro = "Título é obrigatório." });

            if (dto.LancadoNoBrasil && string.IsNullOrWhiteSpace(dto.Editora))
                return BadRequest(new { erro = "Informe a editora se o mangá for lançado no Brasil." });

            try
            {
                var manga = _service.CadastrarNoCatalogo(
                    dto.Titulo.Trim(),
                    dto.LancadoNoBrasil,
                    dto.LancadoNoBrasil ? dto.Editora?.Trim() : null
                );

                return CreatedAtAction(nameof(GetById), new { id = manga.Id }, manga);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        // =========================
        // PUT: api/catalogo/{id} (admin) - edição rápida
        // =========================
        [HttpPut("{id:guid}")]
        public IActionResult Put(
            [FromHeader(Name = "X-User-Id")] Guid? userId,
            Guid id,
            [FromBody] AtualizarMangaDto dto)
        {
            if (!TrySetAdmin(userId, out var erro))
                return erro!;

            if (dto is null || string.IsNullOrWhiteSpace(dto.Titulo))
                return BadRequest(new { erro = "Título é obrigatório." });

            if (dto.LancadoNoBrasil && string.IsNullOrWhiteSpace(dto.Editora))
                return BadRequest(new { erro = "Informe a editora se o mangá for lançado no Brasil." });

            try
            {
                var manga = _service.AtualizarMangaDoCatalogo(
                    id,
                    dto.Titulo.Trim(),
                    dto.LancadoNoBrasil,
                    dto.LancadoNoBrasil ? dto.Editora?.Trim() : null
                );

                return Ok(manga);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        // =========================
        // PUT: api/catalogo/{id}/detalhes (admin) - detalhes avançados
        // =========================
        [HttpPut("{id:guid}/detalhes")]
        public IActionResult PutDetalhes(
            [FromHeader(Name = "X-User-Id")] Guid? userId,
            Guid id,
            [FromBody] AtualizarMangaDetalhesDto dto)
        {
            if (!TrySetAdmin(userId, out var erro))
                return erro!;

            if (dto is null)
                return BadRequest(new { erro = "Body inválido." });

            if (!UrlValida(dto.CapaUrl))
                return BadRequest(new { erro = "CapaUrl inválida. Use um link http/https." });

            if (!AnoValido(dto.AnoLancamentoOriginal) || !AnoValido(dto.AnoLancamentoBrasil))
                return BadRequest(new { erro = "Ano inválido (use entre 1900 e ano atual + 1)." });

            // Se quiser obrigar AnoLancamentoBrasil quando LancadoNoBrasil = true,
            // isso é regra de negócio (pode ser no service). Por enquanto deixo opcional.

            try
            {
                // ✅ IMPORTANTE:
                // Este método precisa existir no service.
                // No próximo passo eu te passo exatamente como criar no IBibliotecaService e no BibliotecaServiceDb.
                var manga = _service.AtualizarDetalhesManga(
                    id,
                    dto.CapaUrl?.Trim(),
                    dto.Descricao?.Trim(),
                    dto.Demografia?.Trim(),
                    dto.Autor?.Trim(),
                    dto.AnoLancamentoOriginal,
                    dto.AnoLancamentoBrasil
                );

                return Ok(manga);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        // =========================
        // DELETE: api/catalogo/{id} (admin)
        // =========================
        [HttpDelete("{id:guid}")]
        public IActionResult Delete(
            [FromHeader(Name = "X-User-Id")] Guid? userId,
            Guid id)
        {
            if (!TrySetAdmin(userId, out var erro))
                return erro!;

            try
            {
                _service.RemoverMangaDoCatalogo(id);
                return Ok(new { mensagem = "Mangá removido do catálogo." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }
    }
}
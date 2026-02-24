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

        // DTOs (somente campos que você quer usar agora)
        public record CadastrarMangaDto(string Titulo, bool LancadoNoBrasil, string? Editora);
        public record AtualizarMangaDto(string Titulo, bool LancadoNoBrasil, string? Editora);

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
        public ActionResult<Manga> Post(
            [FromHeader(Name = "X-User-Id")] Guid? userId,
            [FromBody] CadastrarMangaDto dto)
        {
            // Header obrigatório
            if (userId is null)
                return Unauthorized(new { erro = "Usuário não informado no header X-User-Id." });

            // Define usuário atual no service
            if (!_service.DefinirUsuarioAtual(userId.Value))
                return Unauthorized(new { erro = "Usuário não encontrado." });

            // Confere login + admin
            var usuarioLogado = _service.ObterUsuarioLogado();
            if (usuarioLogado is null)
                return Unauthorized(new { erro = "Faça login como admin para cadastrar mangás." });

            if (!usuarioLogado.EhAdmin)
                return StatusCode(403, new { erro = "Apenas administradores podem cadastrar mangás." });

            // Valida DTO
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
        // PUT: api/catalogo/{id} (admin)
        // =========================
        [HttpPut("{id:guid}")]
        public ActionResult<Manga> Put(
            [FromHeader(Name = "X-User-Id")] Guid? userId,
            Guid id,
            [FromBody] AtualizarMangaDto dto)
        {
            if (userId is null)
                return Unauthorized(new { erro = "Usuário não informado no header X-User-Id." });

            if (!_service.DefinirUsuarioAtual(userId.Value))
                return Unauthorized(new { erro = "Usuário não encontrado." });

            var usuarioLogado = _service.ObterUsuarioLogado();
            if (usuarioLogado is null)
                return Unauthorized(new { erro = "Faça login como admin." });

            if (!usuarioLogado.EhAdmin)
                return StatusCode(403, new { erro = "Apenas administradores podem editar mangás." });

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
        // DELETE: api/catalogo/{id} (admin)
        // =========================
        [HttpDelete("{id:guid}")]
        public IActionResult Delete(
            [FromHeader(Name = "X-User-Id")] Guid? userId,
            Guid id)
        {
            if (userId is null)
                return Unauthorized(new { erro = "Usuário não informado no header X-User-Id." });

            if (!_service.DefinirUsuarioAtual(userId.Value))
                return Unauthorized(new { erro = "Usuário não encontrado." });

            var usuarioLogado = _service.ObterUsuarioLogado();
            if (usuarioLogado is null)
                return Unauthorized(new { erro = "Faça login como admin." });

            if (!usuarioLogado.EhAdmin)
                return StatusCode(403, new { erro = "Apenas administradores podem excluir mangás." });

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
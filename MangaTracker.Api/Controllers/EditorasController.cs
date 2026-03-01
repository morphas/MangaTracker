using Microsoft.AspNetCore.Mvc;
using MangaTracker.Services;

namespace MangaTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EditorasController : ControllerBase
    {
        private readonly IBibliotecaService _service;

        public EditorasController(IBibliotecaService service)
        {
            _service = service;
        }

        public record CriarEditoraDto(string Nome, string? Descricao);
        public record AtualizarEditoraDto(string Nome, string? Descricao);

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

            var usuario = _service.ObterUsuarioLogado();
            if (usuario is null)
            {
                erro = Unauthorized(new { erro = "Faça login como admin." });
                return false;
            }

            if (!usuario.EhAdmin)
            {
                erro = StatusCode(403, new { erro = "Apenas administradores podem executar esta ação." });
                return false;
            }

            return true;
        }

        // GET: /api/editoras  (público - pra popular select)
        [HttpGet]
        public IActionResult Get()
        {
            var lista = _service.ListarEditoras()
                .OrderBy(e => e.Nome, StringComparer.CurrentCultureIgnoreCase)
                .Select(e => new
                {
                    id = e.Id,
                    nome = e.Nome,
                    key = e.Key,
                    descricao = e.Descricao
                })
                .ToList();

            return Ok(lista);
        }

        // POST: /api/editoras (admin)
        [HttpPost]
        public IActionResult Post(
            [FromHeader(Name = "X-User-Id")] Guid? userId,
            [FromBody] CriarEditoraDto dto)
        {
            if (!TrySetAdmin(userId, out var erro))
                return erro!;

            if (dto is null || string.IsNullOrWhiteSpace(dto.Nome))
                return BadRequest(new { erro = "Nome é obrigatório." });

            try
            {
                var editora = _service.CriarEditora(dto.Nome.Trim(), dto.Descricao?.Trim());
                return Ok(new
                {
                    id = editora.Id,
                    nome = editora.Nome,
                    key = editora.Key,
                    descricao = editora.Descricao
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        // PUT: /api/editoras/{id} (admin)
        [HttpPut("{id:guid}")]
        public IActionResult Put(
            [FromHeader(Name = "X-User-Id")] Guid? userId,
            Guid id,
            [FromBody] AtualizarEditoraDto dto)
        {
            if (!TrySetAdmin(userId, out var erro))
                return erro!;

            if (dto is null || string.IsNullOrWhiteSpace(dto.Nome))
                return BadRequest(new { erro = "Nome é obrigatório." });

            try
            {
                var editora = _service.AtualizarEditora(id, dto.Nome.Trim(), dto.Descricao?.Trim());
                return Ok(new
                {
                    id = editora.Id,
                    nome = editora.Nome,
                    key = editora.Key,
                    descricao = editora.Descricao
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        // DELETE: /api/editoras/{id} (admin)
        // ✅ Bloqueado se tiver mangá vinculado (a regra está no service)
        [HttpDelete("{id:guid}")]
        public IActionResult Delete(
            [FromHeader(Name = "X-User-Id")] Guid? userId,
            Guid id)
        {
            if (!TrySetAdmin(userId, out var erro))
                return erro!;

            try
            {
                _service.RemoverEditora(id);
                return Ok(new { mensagem = "Editora removida." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }
    }
}
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

        public record CadastrarMangaDto(string Titulo, int? TotalCapitulos);

        // GET: api/catalogo
        [HttpGet]
        public ActionResult<IEnumerable<Manga>> Get()
        {
            return Ok(_service.ListarCatalogo());
        }

        // GET: api/catalogo/{id}
        [HttpGet("{id:guid}")]
        public ActionResult<Manga> GetById(Guid id)
        {
            var manga = _service.BuscarMangaPorId(id);
            if (manga is null) return NotFound(new { erro = "Mangá não encontrado." });
            return Ok(manga);
        }

        // POST: api/catalogo  (admin apenas)
        [HttpPost]
        public ActionResult<Manga> Post(
            [FromHeader(Name = "X-User-Id")] Guid? userId,
            [FromBody] CadastrarMangaDto dto)
        {
            if (userId is null)
                return Unauthorized(new { erro = "Usuário não informado no header X-User-Id." });

            if (!_service.DefinirUsuarioAtual(userId.Value))
                return Unauthorized(new { erro = "Usuário não encontrado." });

            var usuarioLogado = _service.ObterUsuarioLogado();
            if (usuarioLogado is null)
                return Unauthorized(new { erro = "Faça login como admin para cadastrar mangás." });

            if (!usuarioLogado.EhAdmin)
                return StatusCode(403, new { erro = "Apenas administradores podem cadastrar mangás." });

            if (dto is null || string.IsNullOrWhiteSpace(dto.Titulo))
                return BadRequest(new { erro = "Título é obrigatório." });

            try
            {
                var manga = _service.CadastrarNoCatalogo(dto.Titulo, dto.TotalCapitulos);
                return CreatedAtAction(nameof(GetById), new { id = manga.Id }, manga);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }
    }
}
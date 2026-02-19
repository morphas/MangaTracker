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

        [HttpGet]
        public ActionResult<IEnumerable<Manga>> Get()
        {
            return Ok(_service.ListarCatalogo());
        }

        [HttpGet("{id:guid}")]
        public ActionResult<Manga> GetById(Guid id)
        {
            var manga = _service.BuscarMangaPorId(id);
            if (manga is null) return NotFound();
            return Ok(manga);
        }

        [HttpPost]
        public ActionResult<Manga> Post([FromBody] CadastrarMangaDto dto)
        {
            // Esta linha deve usar 'ObterUsuarioLogado' exatamente como na sua Interface
            var usuarioLogado = _service.ObterUsuarioLogado();

            if (usuarioLogado == null || !usuarioLogado.EhAdmin)
            {
                return Forbid();
            }

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
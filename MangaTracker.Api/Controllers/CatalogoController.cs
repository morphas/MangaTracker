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

        // GET: api/catalogo
        [HttpGet]
        public ActionResult<IEnumerable<Manga>> Get()
        {
            var lista = _service
                .ListarCatalogo()
                .OrderBy(m => m.Titulo, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            return Ok(lista);
        }

        // GET: api/catalogo/{id}
        [HttpGet("{id:guid}")]
        public ActionResult<Manga> GetById(Guid id)
        {
            var manga = _service.BuscarMangaPorId(id);
            if (manga is null)
                return NotFound(new { erro = "Mangá não encontrado." });

            return Ok(manga);
        }

        public record CadastrarMangaDto(string Titulo, int? TotalCapitulos);

        // POST: api/catalogo
        [HttpPost]
        public ActionResult<Manga> Post([FromBody] CadastrarMangaDto dto)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.Titulo))
                return BadRequest(new { erro = "Título é obrigatório." });

            if (dto.TotalCapitulos.HasValue && dto.TotalCapitulos.Value < 1)
                return BadRequest(new { erro = "TotalCapitulos deve ser >= 1." });

            try
            {
                var manga = _service.CadastrarNoCatalogo(dto.Titulo, dto.TotalCapitulos);

                // retorna 201 + Location apontando para o GET por id
                return CreatedAtAction(nameof(GetById), new { id = manga.Id }, manga);
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }
    }
}

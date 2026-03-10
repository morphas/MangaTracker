using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MangaTracker.Api.Data;

namespace MangaTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HomeController : ControllerBase
    {
        private readonly MangaTrackerDbContext _db;

        public HomeController(MangaTrackerDbContext db)
        {
            _db = db;
        }

        [HttpGet("ranking/{tipo}")]
        public IActionResult GetRanking(string tipo)
        {
            var dados = _db.RankingHomes
                .AsNoTracking()
                .Where(r => r.Tipo == tipo)
                .Join(
                    _db.Catalogo,
                    ranking => ranking.MangaId,
                    manga => manga.Id,
                    (ranking, manga) => new
                    {
                        posicao = ranking.Posicao,
                        valor = ranking.Valor,
                        mangaId = manga.Id,
                        titulo = manga.Titulo,
                        geradoEm = ranking.GeradoEm
                    })
                .OrderBy(x => x.posicao)
                .ToList();

            return Ok(dados);
        }
    }
}
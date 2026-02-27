using Microsoft.AspNetCore.Mvc;
using MangaTracker.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace MangaTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminLogsController : ControllerBase
    {
        private readonly MangaTrackerDbContext _db;

        public AdminLogsController(MangaTrackerDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var logs = _db.AdminLogs
                .AsNoTracking()
                .OrderByDescending(l => l.CriadoEm)
                .Take(100)
                .Select(l => new
                {
                    acao = l.Acao,
                    detalhes = l.Detalhes,
                    criadoEm = l.CriadoEm,
                    adminId = l.AdminId,
                    admin = _db.Usuarios
                        .Where(u => u.Id == l.AdminId)
                        .Select(u => u.Nome)
                        .FirstOrDefault() ?? "—"
                })
                .ToList();

            return Ok(logs);
        }
    }
}
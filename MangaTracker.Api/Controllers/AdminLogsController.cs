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
        public IActionResult GetLogs()
        {
            var logs = _db.AdminLogs
                .OrderByDescending(l => l.CriadoEm)
                .Take(100)
                .AsNoTracking()
                .ToList();

            return Ok(logs);
        }
    }
}
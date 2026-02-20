using Microsoft.AspNetCore.Mvc;
using MangaTracker.Models;
using MangaTracker.Services;

namespace MangaTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MinhaListaController : ControllerBase
    {
        private readonly IBibliotecaService _service;

        public MinhaListaController(IBibliotecaService service)
        {
            _service = service;
        }

        private bool TrySetUser(Guid? userId, out IActionResult? erro)
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

            return true;
        }

        // DTO de retorno
        public record MinhaLeituraDto(
            Guid MangaId,
            string Titulo,
            int? TotalCapitulos,
            StatusLeitura Status,
            int CapituloAtual,
            DateTime? UltimaLeituraEm
        );

        // POST: api/minhalista
        public record AdicionarMinhaListaDto(Guid MangaId, int Status, int? CapituloAtual);

        // PUT: api/minhalista/{mangaId}
        public record AtualizarProgressoDto(int CapituloAtual, int? Status);

        // GET: api/minhalista
        [HttpGet]
        public IActionResult Get([FromHeader(Name = "X-User-Id")] Guid? userId)
        {
            if (!TrySetUser(userId, out var erro))
                return erro!;

            var lista = _service.ListarMinhaLista()
                .OrderBy(x => x.Manga.Titulo, StringComparer.CurrentCultureIgnoreCase)
                .Select(x => new MinhaLeituraDto(
                    x.Manga.Id,
                    x.Manga.Titulo,
                    x.Manga.TotalCapitulos,
                    x.Leitura.Status,
                    x.Leitura.CapituloAtual,
                    x.Leitura.UltimaLeituraEm
                ))
                .ToList();

            return Ok(lista);
        }

        // GET: api/minhalista/status/2
        [HttpGet("status/{status:int}")]
        public IActionResult GetPorStatus([FromHeader(Name = "X-User-Id")] Guid? userId, int status)
        {
            if (!TrySetUser(userId, out var erro))
                return erro!;

            if (!Enum.IsDefined(typeof(StatusLeitura), status))
                return BadRequest(new { erro = "Status inválido. Use 1, 2 ou 3." });

            var st = (StatusLeitura)status;

            var lista = _service.ListarMinhaLista()
                .Where(x => x.Leitura.Status == st)
                .OrderBy(x => x.Manga.Titulo, StringComparer.CurrentCultureIgnoreCase)
                .Select(x => new MinhaLeituraDto(
                    x.Manga.Id,
                    x.Manga.Titulo,
                    x.Manga.TotalCapitulos,
                    x.Leitura.Status,
                    x.Leitura.CapituloAtual,
                    x.Leitura.UltimaLeituraEm
                ))
                .ToList();

            return Ok(lista);
        }

        // POST: api/minhalista
        [HttpPost]
        public IActionResult Post(
            [FromHeader(Name = "X-User-Id")] Guid? userId,
            [FromBody] AdicionarMinhaListaDto dto)
        {
            if (!TrySetUser(userId, out var erro))
                return erro!;

            if (dto.MangaId == Guid.Empty)
                return BadRequest(new { erro = "MangaId é obrigatório." });

            if (!Enum.IsDefined(typeof(StatusLeitura), dto.Status))
                return BadRequest(new { erro = "Status inválido. Use 1, 2 ou 3." });

            var status = (StatusLeitura)dto.Status;

            try
            {
                _service.AdicionarNaMinhaLista(dto.MangaId, status, dto.CapituloAtual);
                return Ok(new { mensagem = "Adicionado à sua lista!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        // PUT: api/minhalista/{mangaId}
        [HttpPut("{mangaId:guid}")]
        public IActionResult Put(
            [FromHeader(Name = "X-User-Id")] Guid? userId,
            Guid mangaId,
            [FromBody] AtualizarProgressoDto dto)
        {
            if (!TrySetUser(userId, out var erro))
                return erro!;

            if (mangaId == Guid.Empty)
                return BadRequest(new { erro = "mangaId inválido." });

            StatusLeitura? novoStatus = null;
            if (dto.Status.HasValue)
            {
                if (!Enum.IsDefined(typeof(StatusLeitura), dto.Status.Value))
                    return BadRequest(new { erro = "Status inválido. Use 1, 2 ou 3." });

                novoStatus = (StatusLeitura)dto.Status.Value;
            }

            try
            {
                _service.AtualizarLeitura(mangaId, dto.CapituloAtual, novoStatus);
                return Ok(new { mensagem = "Progresso atualizado!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }
    }
}
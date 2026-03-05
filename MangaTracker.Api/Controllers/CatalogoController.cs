using Microsoft.AspNetCore.Mvc;
using MangaTracker.Services;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public record CadastrarMangaDto(string Titulo, bool LancadoNoBrasil, Guid? EditoraId);
        public record AtualizarMangaDto(string Titulo, bool LancadoNoBrasil, Guid? EditoraId);

        // ✅ Detalhes avançados - com Generos (lista)
        public record AtualizarMangaDetalhesDto(
            string? CapaUrl,
            string? Descricao,
            string? Demografia,
            string? Autor,
            int? AnoLancamentoOriginal,
            int? AnoLancamentoBrasil,
            List<string>? Generos
        );

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
            if (string.IsNullOrWhiteSpace(url)) return true;
            return Uri.TryCreate(url.Trim(), UriKind.Absolute, out var u)
                   && (u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps);
        }

        private static bool AnoValido(int? ano)
        {
            if (!ano.HasValue) return true;
            var a = ano.Value;
            var max = DateTime.Now.Year + 1;
            return a >= 1900 && a <= max;
        }

        // ✅ Normaliza lista: tira vazios, trim, remove duplicados (case-insensitive) e limita a 4
        private static List<string> NormalizarGeneros(List<string>? generos)
        {
            if (generos is null) return new List<string>();

            return generos
                .Select(x => (x ?? "").Trim())
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Take(4)
                .ToList();
        }

        // GET: api/catalogo (PAGINADO)
        [HttpGet]
        public IActionResult Get(
            [FromQuery] bool? lancadoNoBrasil,
            [FromQuery] Guid? editoraId,
            [FromQuery] string? q,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = _service.ListarCatalogoPaginado(lancadoNoBrasil, editoraId, q, page, pageSize);

            var items = result.Items.Select(m => new
            {
                id = m.Id,
                titulo = m.Titulo,
                totalCapitulos = m.TotalCapitulos,
                lancadoNoBrasil = m.LancadoNoBrasil,
                editoraId = m.EditoraId,
                editoraNome = m.EditoraNav?.Nome,
                capaUrl = m.CapaUrl,
                descricao = m.Descricao,
                demografia = m.Demografia,
                autor = m.Autor,
                anoLancamentoOriginal = m.AnoLancamentoOriginal,
                anoLancamentoBrasil = m.AnoLancamentoBrasil,
                generos = m.Generos ?? new List<string>(),
                criadoEm = m.CriadoEm
            });

            return Ok(new
            {
                items,
                total = result.Total,
                page = result.Page,
                pageSize = result.PageSize
            });
        }

        // GET: api/catalogo/{id}
        [HttpGet("{id:guid}")]
        public IActionResult GetById(Guid id)
        {
            var manga = _service.BuscarMangaPorId(id);
            if (manga is null)
                return NotFound(new { erro = "Mangá não encontrado." });

            return Ok(new
            {
                id = manga.Id,
                titulo = manga.Titulo,
                totalCapitulos = manga.TotalCapitulos,
                lancadoNoBrasil = manga.LancadoNoBrasil,
                editoraId = manga.EditoraId,
                editoraNome = manga.EditoraNav?.Nome,
                capaUrl = manga.CapaUrl,
                descricao = manga.Descricao,
                demografia = manga.Demografia,
                autor = manga.Autor,
                anoLancamentoOriginal = manga.AnoLancamentoOriginal,
                anoLancamentoBrasil = manga.AnoLancamentoBrasil,
                generos = manga.Generos ?? new List<string>(),
                criadoEm = manga.CriadoEm
            });
        }

        // POST: api/catalogo (admin)
        [HttpPost]
        public IActionResult Post(
            [FromHeader(Name = "X-User-Id")] Guid? userId,
            [FromBody] CadastrarMangaDto dto)
        {
            if (!TrySetAdmin(userId, out var erro))
                return erro!;

            if (dto is null || string.IsNullOrWhiteSpace(dto.Titulo))
                return BadRequest(new { erro = "Título é obrigatório." });

            if (dto.LancadoNoBrasil && (!dto.EditoraId.HasValue || dto.EditoraId.Value == Guid.Empty))
                return BadRequest(new { erro = "Informe a editora se o mangá for lançado no Brasil." });

            try
            {
                var manga = _service.CadastrarNoCatalogo(
                    dto.Titulo.Trim(),
                    dto.LancadoNoBrasil,
                    dto.LancadoNoBrasil ? dto.EditoraId : null
                );

                return CreatedAtAction(nameof(GetById), new { id = manga.Id }, new
                {
                    id = manga.Id,
                    titulo = manga.Titulo,
                    lancadoNoBrasil = manga.LancadoNoBrasil,
                    editoraId = manga.EditoraId,
                    editoraNome = manga.EditoraNav?.Nome
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        // PUT: api/catalogo/{id} (admin) - edição rápida
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

            if (dto.LancadoNoBrasil && (!dto.EditoraId.HasValue || dto.EditoraId.Value == Guid.Empty))
                return BadRequest(new { erro = "Informe a editora se o mangá for lançado no Brasil." });

            try
            {
                var manga = _service.AtualizarMangaDoCatalogo(
                    id,
                    dto.Titulo.Trim(),
                    dto.LancadoNoBrasil,
                    dto.LancadoNoBrasil ? dto.EditoraId : null
                );

                return Ok(new
                {
                    id = manga.Id,
                    titulo = manga.Titulo,
                    lancadoNoBrasil = manga.LancadoNoBrasil,
                    editoraId = manga.EditoraId,
                    editoraNome = manga.EditoraNav?.Nome
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        // PUT: api/catalogo/{id}/detalhes (admin) - detalhes avançados
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

            var generosOk = NormalizarGeneros(dto.Generos);

            try
            {
                var manga = _service.AtualizarDetalhesManga(
                    id,
                    dto.CapaUrl?.Trim(),
                    dto.Descricao?.Trim(),
                    dto.Demografia?.Trim(),
                    dto.Autor?.Trim(),
                    dto.AnoLancamentoOriginal,
                    dto.AnoLancamentoBrasil,
                    generosOk
                );

                return Ok(new
                {
                    id = manga.Id,
                    titulo = manga.Titulo,
                    capaUrl = manga.CapaUrl,
                    descricao = manga.Descricao,
                    demografia = manga.Demografia,
                    autor = manga.Autor,
                    anoLancamentoOriginal = manga.AnoLancamentoOriginal,
                    anoLancamentoBrasil = manga.AnoLancamentoBrasil,
                    generos = manga.Generos ?? new List<string>()
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        // DELETE: api/catalogo/{id} (admin)
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
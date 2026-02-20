using Microsoft.AspNetCore.Mvc;
using MangaTracker.Services;

namespace MangaTracker.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsuarioController : ControllerBase
    {
        private readonly IBibliotecaService _service;

        public UsuarioController(IBibliotecaService service)
        {
            _service = service;
        }

        // POST: api/usuario/cadastrar
        [HttpPost("cadastrar")]
        public IActionResult Cadastrar([FromBody] CadastroDto dados)
        {
            try
            {
                _service.CadastrarNovoUsuario(dados.Nome, dados.Email, dados.Senha);
                return Ok(new { mensagem = "Usuário criado com sucesso!" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        // POST: api/usuario/login
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto dados)
        {
            try
            {
                var usuario = _service.ValidarLogin(dados.Identificador, dados.Senha);

                return Ok(new
                {
                    id = usuario.Id,
                    nome = usuario.Nome,
                    isAdmin = usuario.EhAdmin
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }

        // GET: api/usuario/atual
        // FASE 1 - usa header X-User-Id tipado
        [HttpGet("atual")]
        public IActionResult UsuarioAtual([FromHeader(Name = "X-User-Id")] Guid? userId)
        {
            if (userId == null)
                return Ok(new { logado = false });

            if (!_service.DefinirUsuarioAtual(userId.Value))
                return Ok(new { logado = false });

            var usuario = _service.ObterUsuarioLogado();
            if (usuario == null)
                return Ok(new { logado = false });

            return Ok(new
            {
                logado = true,
                id = usuario.Id,
                nome = usuario.Nome,
                isAdmin = usuario.EhAdmin
            });
        }
    }

    public record CadastroDto(string Nome, string Email, string Senha);
    public record LoginDto(string Identificador, string Senha);
}
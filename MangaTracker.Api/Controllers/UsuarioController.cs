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

        // Função para Criar Usuário
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

        // Função para Fazer Login (Morphas ou Email)
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto dados)
        {
            try
            {
                var usuario = _service.ValidarLogin(dados.Identificador, dados.Senha);
                return Ok(new { nome = usuario.Nome, isAdmin = usuario.EhAdmin });
            }
            catch (Exception ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
        }
    }

    // Pacotinhos de dados (Records)
    public record CadastroDto(string Nome, string Email, string Senha);
    public record LoginDto(string Identificador, string Senha);
}
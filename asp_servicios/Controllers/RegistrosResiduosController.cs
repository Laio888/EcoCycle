using Aplicacion.Interfaces;
using Dominio.Entidades;
using Microsoft.AspNetCore.Mvc;

namespace asp_servicios.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegistrosResiduosController : ControllerBase
{
    private readonly IRegistroResiduoAplicacion _registroResiduoAplicacion;

    public RegistrosResiduosController(IRegistroResiduoAplicacion registroResiduoAplicacion)
    {
        _registroResiduoAplicacion = registroResiduoAplicacion;
    }

    [HttpPost("Registrar")]
    public IActionResult Registrar([FromBody] RegistroResiduoRequest request)
    {
        if (request == null)
        {
            return BadRequest("Solicitud inválida.");
        }

        if (request.PesoKg <= 0)
        {
            return BadRequest("El peso del residuo debe ser mayor que 0.");
        }

        if (request.UsuarioId <= 0)
        {
            return BadRequest("Debe especificar un usuario válido.");
        }

        var registro = new RegistrosResiduos
        {
            UsuarioId = request.UsuarioId,
            TipoResiduoId = request.TipoResiduoId,
            PesoKg = request.PesoKg,
            EvidenciaArchivoId = request.EvidenciaArchivoId
        };

        var resultado = _registroResiduoAplicacion.Registrar(registro);
        return Ok(new { Mensaje = "Registro guardado", RegistroId = resultado.RegistroResiduoId });
    }

    [HttpGet("Usuario/{usuarioId}")]
    public IActionResult ListarPorUsuario(int usuarioId)
    {
        var lista = _registroResiduoAplicacion.ListarPorUsuario(usuarioId);
        return Ok(lista);
    }
}

    public class RegistroResiduoRequest
    {
        public int UsuarioId { get; set; }
        public int TipoResiduoId { get; set; }
        public decimal PesoKg { get; set; }
        public int? EvidenciaArchivoId { get; set; }
    }
}

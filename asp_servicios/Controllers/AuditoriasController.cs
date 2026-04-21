using Aplicacion.Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuditoriasController : ControllerBase
{
    private readonly IAuditoriaAplicacion _auditoriaAplicacion;
    public AuditoriasController(IAuditoriaAplicacion auditoriaAplicacion)
    {
        _auditoriaAplicacion = auditoriaAplicacion;
    }

    [HttpGet("Listar")]
    public IActionResult Listar()
    {
        var lista = _auditoriaAplicacion.ListarPorEntidad("", null, 200);
        return Ok(new { Entidades = lista });
    }

    [HttpPost("PorEntidad")]
    public IActionResult PorEntidad([FromBody] dynamic data)
    {
        string entidad = data?.Entidad ?? "";
        int? entidadId = data?.EntidadId != null ? (int?)data.EntidadId : null;
        var lista = _auditoriaAplicacion.ListarPorEntidad(entidad, entidadId);
        return Ok(new { Entidades = lista });
    }
}

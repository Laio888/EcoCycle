using Dominio.Entidades;

namespace Aplicacion.Interfaces
{
    public interface IAuditoriaAplicacion
    {
        Auditorias Registrar(Auditorias auditoria);
        List<Auditorias> ListarPorEntidad(string entidad, int? entidadId = null, int take = 100);
    }
}

using Dominio.Entidades;

namespace Presentaciones.Interfaces
{
    public interface IAuditoriasPresentacion
    {
        Task<List<Auditorias>> Listar();
        // entidadId opcional para mayor flexibilidad
        Task<List<Auditorias>> PorEntidad(string entidad, int? entidadId);
    }
}

using Dominio.Entidades;

namespace Presentaciones.Interfaces
{
    public interface IUsuariosPresentacion
    {
        Task<List<Usuarios>> Listar();
        Task<List<Usuarios>> PorUsuario(Usuarios? entidad);
        Task<Usuarios?> Guardar(Usuarios? entidad);
        Task<Usuarios?> Modificar(Usuarios? entidad);
        Task<Usuarios?> Borrar(Usuarios? entidad);
    }
}

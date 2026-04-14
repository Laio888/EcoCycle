using Dominio.Entidades;

public interface IUsuariosAplicacion
{
    void Configurar(string StringConexion);

    List<Usuarios> Listar();
    Usuarios? Guardar(Usuarios? entidad);
    Usuarios? Modificar(Usuarios? entidad);
    Usuarios? Borrar(Usuarios? entidad);

    Usuarios? ObtenerPorId(int id);
}

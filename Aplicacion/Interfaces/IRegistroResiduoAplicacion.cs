using Dominio.Entidades;

namespace Aplicacion.Interfaces
{
    public interface IRegistroResiduoAplicacion
    {
        RegistrosResiduos Registrar(RegistrosResiduos registro);
        IEnumerable<RegistrosResiduos> ListarPorUsuario(int usuarioId);
    }
}

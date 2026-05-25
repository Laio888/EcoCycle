using Aplicacion.Interfaces;
using Dominio.Entidades;
using Repositorio.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace Aplicacion.Implementaciones
{
    public class RegistroResiduoAplicacion : IRegistroResiduoAplicacion
    {
        private readonly IConexion _conexion;

        public RegistroResiduoAplicacion(IConexion conexion)
        {
            _conexion = conexion;
        }

        public RegistrosResiduos Registrar(RegistrosResiduos registro)
        {
            if (registro == null)
            {
                throw new ArgumentNullException(nameof(registro));
            }

            if (_conexion.RegistrosResiduos == null)
            {
                throw new InvalidOperationException("El contexto de RegistrosResiduos no está disponible.");
            }

            _conexion.RegistrosResiduos.Add(registro);
            _conexion.SaveChanges();
            return registro;
        }

        public IEnumerable<RegistrosResiduos> ListarPorUsuario(int usuarioId)
        {
            if (_conexion.RegistrosResiduos == null)
            {
                return Enumerable.Empty<RegistrosResiduos>();
            }

            return _conexion.RegistrosResiduos.Where(r => r.UsuarioId == usuarioId).ToList();
        }
    }
}

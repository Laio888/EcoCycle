using Dominio.Entidades;
using Aplicacion.Interfaces;
using Repositorio.Interfaces;
using Dominio.Nucleo;
using System.Linq;

namespace Aplicacion.Implementaciones
{
    public class AuditoriaAplicacion : IAuditoriaAplicacion
    {
        private readonly IConexion _conexion;
        public AuditoriaAplicacion(IConexion conexion) { _conexion = conexion; }

        public Auditorias Registrar(Auditorias auditoria)
        {
            _conexion.Auditorias!.Add(auditoria);
            _conexion.SaveChanges();
            return auditoria;
        }

        public List<Auditorias> ListarPorEntidad(string entidad, int? entidadId = null, int take = 100)
        {
            var q = _conexion.Auditorias!.Where(a => a.Entidad == entidad);
            if (entidadId.HasValue) q = q.Where(a => a.EntidadId == entidadId.Value);
            return q.OrderByDescending(a => a.FechaAccion).Take(take).ToList();
        }
    }
}
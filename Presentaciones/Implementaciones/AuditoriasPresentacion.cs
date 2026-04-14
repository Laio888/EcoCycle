using Dominio.Nucleo;
using Dominio.Entidades;
using Presentaciones.Interfaces;
using System.Threading.Tasks;

namespace Presentaciones.Implementaciones
{
    public class AuditoriasPresentacion : IAuditoriasPresentacion
    {
        private Comunicaciones? comunicaciones = null;

        public async Task<List<Auditorias>> Listar()
        {
            var lista = new List<Auditorias>();
            var datos = new Dictionary<string, object>();
            comunicaciones = new Comunicaciones();
            datos = comunicaciones.ConstruirUrl(datos, "Auditorias/Listar");

            var respuesta = await comunicaciones.Execute(datos)
                ?? throw new Exception("lbErrorComunicacion");

            if (respuesta.TryGetValue("Error", out var err))
                throw new Exception(err?.ToString() ?? "lbErrorDesconocido");

            if (!respuesta.TryGetValue("Entidades", out var entidadesObj) || entidadesObj == null)
                return new List<Auditorias>();

            lista = JsonConversor.ConvertirAObjeto<List<Auditorias>>(
                JsonConversor.ConvertirAString(entidadesObj));

            return lista;
        }

        public async Task<List<Auditorias>> PorEntidad(string entidad, int? entidadId)
        {
            var lista = new List<Auditorias>();
            var datos = new Dictionary<string, object>
            {
                ["Entidad"] = entidad
            };
            if (entidadId.HasValue) datos["EntidadId"] = entidadId.Value;

            comunicaciones = new Comunicaciones();
            datos = comunicaciones.ConstruirUrl(datos, "Auditorias/PorEntidad");

            var respuesta = await comunicaciones.Execute(datos)
                ?? throw new Exception("lbErrorComunicacion");

            if (respuesta.TryGetValue("Error", out var err))
                throw new Exception(err?.ToString() ?? "lbErrorDesconocido");

            if (!respuesta.TryGetValue("Entidades", out var entidadesObj) || entidadesObj == null)
                return new List<Auditorias>();

            lista = JsonConversor.ConvertirAObjeto<List<Auditorias>>(
                JsonConversor.ConvertirAString(entidadesObj));

            return lista;
        }

        // Método adicional para permitir logging local (RegistrarAsync)

        public async Task RegistrarAsync(Auditorias auditoria)
        {
            // delega en un logger local que escribe archivo.
            var logger = new AuditoriasLogger();
            await logger.RegistrarAsync(auditoria);
        }
    }
}

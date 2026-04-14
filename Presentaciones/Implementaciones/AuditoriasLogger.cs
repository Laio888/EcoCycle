using Dominio.Entidades;
using System.Text;
using System.IO;

namespace Presentaciones.Implementaciones
{
    public class AuditoriasLogger
    {
        private readonly string rutaArchivo;

        public AuditoriasLogger(string rutaPersonalizada = "")
        {
            rutaArchivo = string.IsNullOrWhiteSpace(rutaPersonalizada)
                ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "auditorias_log.txt")
                : rutaPersonalizada;
        }

        public async Task RegistrarAsync(Auditorias auditoria)
        {
            if (auditoria == null) return;

            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("────────────────────────────────────────────");
                // Ajusta nombres de propiedades según tu entidad Auditorias
                sb.AppendLine($"Fecha: {(auditoria.FechaAccion != DateTime.MinValue ? auditoria.FechaAccion.ToString("yyyy-MM-dd HH:mm:ss") : DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))}");
                sb.AppendLine($"Usuario: {auditoria.UsuarioAccion}");
                sb.AppendLine($"Entidad: {auditoria.Entidad}");
                sb.AppendLine($"EntidadId: {auditoria.EntidadId}");
                sb.AppendLine($"Acción: {auditoria.Accion}");

                if (!string.IsNullOrEmpty(auditoria.DatosAntes))
                {
                    sb.AppendLine("Datos antes:");
                    sb.AppendLine(auditoria.DatosAntes);
                }

                if (!string.IsNullOrEmpty(auditoria.DatosDespues))
                {
                    sb.AppendLine("Datos después:");
                    sb.AppendLine(auditoria.DatosDespues);
                }

                sb.AppendLine("────────────────────────────────────────────");
                sb.AppendLine();

                var dir = Path.GetDirectoryName(rutaArchivo);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                await File.AppendAllTextAsync(rutaArchivo, sb.ToString(), Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al registrar auditoría: {ex.Message}");
            }
        }
    }
}

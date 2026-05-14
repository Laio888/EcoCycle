using Dominio.Entidades;
using System;

namespace ut_presentacion.Nucleo
{
    public class EntidadesNucleo
    {
        public static Usuarios Usuarios(int? nivelId = null)
        {
            var entidad = new Usuarios();
            var guidSufijo = Guid.NewGuid().ToString("N").Substring(0, 8);
            entidad.CorreoElectronico = $"usuario_{guidSufijo}@ecocycle.com";
            entidad.ContrasenaHash = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Substring(0, 20);
            entidad.FechaRegistro = DateTime.Now;
            entidad.FechaUltimoInicioSesion = null;
            entidad.NivelIdActual = nivelId ?? 1; // Default: NivelId 1 = 'Principiante'
            return entidad;
        }

        public static PreferenciasUsuarios PreferenciasUsuarios(int usuarioId, string clave = "tema", string valor = "oscuro")
        {
            var entidad = new PreferenciasUsuarios();
            entidad.UsuarioId = usuarioId;
            entidad.Clave = clave;
            entidad.Valor = valor;
            // FechaActualizacion se asigna automáticamente en BD (GETDATE())
            return entidad;
        }

        public static TiposResiduos TiposResiduos(int calidadResiduoId = 1)
        {
            var entidad = new TiposResiduos();
            var guidSufijo = Guid.NewGuid().ToString("N").Substring(0, 8);
            entidad.Nombre = $"Residuo_{guidSufijo}";
            entidad.CalidadResiduoId = calidadResiduoId;
            entidad.AporteNutricional = "Alto en nutrientes";
            entidad.RelacionCarbono = 25;
            entidad.RelacionNitrogeno = 1;
            return entidad;
        }

        public static RegistrosResiduos RegistrosResiduos(int usuarioId, int tipoResiduoId, decimal pesoKg = 1.5m, int? evidenciaArchivoId = null)
        {
            var entidad = new RegistrosResiduos();
            entidad.UsuarioId = usuarioId;
            entidad.TipoResiduoId = tipoResiduoId;
            entidad.PesoKg = pesoKg;
            entidad.EvidenciaArchivoId = evidenciaArchivoId;
            // FechaRegistro se asigna automáticamente en BD (GETDATE())
            return entidad;
        }

        public static Recompensas Recompensas(int tipoRecompensaId = 1, int costoPuntos = 100)
        {
            var entidad = new Recompensas();
            var guidSufijo = Guid.NewGuid().ToString("N").Substring(0, 8);
            entidad.Nombre = $"Recompensa_{guidSufijo}";
            entidad.Descripcion = "Descripción de prueba";
            entidad.TipoRecompensaId = tipoRecompensaId;
            entidad.CostoPuntos = costoPuntos;
            entidad.StockDisponible = tipoRecompensaId == 2 ? 10 : (int?)null;
            entidad.EsIlimitado = tipoRecompensaId == 1;
            entidad.FechaVigenciaDesde = DateTime.Today;
            entidad.FechaVigenciaHasta = DateTime.Today.AddMonths(6);
            entidad.ImagenArchivoId = null;
            return entidad;
        }

        public static CanjesRecompensas CanjesRecompensas(int usuarioId, int recompensaId, int puntosGastados = 100)
        {
            var entidad = new CanjesRecompensas();
            entidad.UsuarioId = usuarioId;
            entidad.RecompensaId = recompensaId;
            entidad.PuntosGastados = puntosGastados;
            entidad.ComprobanteArchivoId = null;
            // FechaCanje se asigna automáticamente en BD (GETDATE())
            return entidad;
        }

        public static PuntosHistoricos PuntosHistoricosGanancia(int usuarioId, int registroResiduoOrigenId, int puntosAcumulados = 100)
        {
            var entidad = new PuntosHistoricos();
            entidad.UsuarioId = usuarioId;
            entidad.PuntosAcumulados = puntosAcumulados;
            entidad.Motivo = "Registro de residuo";
            entidad.RegistroResiduoOrigenId = registroResiduoOrigenId;
            entidad.CanjeOrigenId = null;
            // FechaCambio se asigna automáticamente en BD (GETDATE())
            return entidad;
        }

        public static ContenidoEducativo ContenidoEducativo(int tipoContenidoId = 1, int categoriaContenidoId = 1)
        {
            var entidad = new ContenidoEducativo();
            var guidSufijo = Guid.NewGuid().ToString("N").Substring(0, 8);
            entidad.Titulo = $"Contenido_{guidSufijo}";
            entidad.TipoContenidoId = tipoContenidoId;
            entidad.CategoriaContenidoId = categoriaContenidoId;
            entidad.RecursoArchivoId = null;
            entidad.EsExterno = false;
            entidad.FuenteExterna = null;
            // FechaPublicacion se asigna automáticamente en BD (GETDATE())
            return entidad;
        }

        public static UsuariosContenidoVisto UsuariosContenidoVisto(int usuarioId, int contenidoId)
        {
            var entidad = new UsuariosContenidoVisto();
            entidad.UsuarioId = usuarioId;
            entidad.ContenidoId = contenidoId;
            // FechaVisionado se asigna automáticamente en BD (GETDATE())
            return entidad;
        }

        public static Notificaciones Notificaciones(int usuarioId, int tipoNotificacionId = 1)
        {
            var entidad = new Notificaciones();
            entidad.UsuarioId = usuarioId;
            entidad.TipoNotificacionId = tipoNotificacionId;
            entidad.Mensaje = "Mensaje de prueba";
            entidad.Leida = false;
            // FechaEnvio se asigna automáticamente en BD (GETDATE())
            return entidad;
        }

        public static FeedbackUsuarios FeedbackUsuarios(int usuarioId, int tipoFeedbackId = 1, int estadoFeedbackId = 1)
        {
            var entidad = new FeedbackUsuarios();
            entidad.UsuarioId = usuarioId;
            entidad.TipoFeedbackId = tipoFeedbackId;
            entidad.Mensaje = "Feedback de prueba";
            entidad.EstadoFeedbackId = estadoFeedbackId;
            // Fecha se asigna automáticamente en BD (GETDATE())
            return entidad;
        }

        public static Archivos Archivos(int tipoArchivoId = 1)
        {
            var entidad = new Archivos();
            var guidSufijo = Guid.NewGuid().ToString("N").Substring(0, 8);
            entidad.Url = $"https://storage.ecocycle.com/file_{guidSufijo}.pdf";
            entidad.TipoArchivoId = tipoArchivoId;
            entidad.EsExterno = tipoArchivoId == 4;
            entidad.Proveedor = "Local";
            entidad.Descripcion = "Archivo de prueba";
            // FechaCreacion se asigna automáticamente en BD (GETDATE())
            return entidad;
        }

        public static class TiposNotificacionIds
        {
            public const int Recordatorio = 1;
            public const int Logro = 2;
            public const int RecompensaDisponible = 3;
        }

        public static class TiposFeedbackIds
        {
            public const int Sugerencia = 1;
            public const int Problema = 2;
            public const int Idea = 3;
        }

        public static class EstadosFeedbackIds
        {
            public const int Pendiente = 1;
            public const int Resuelto = 2;
        }

        public static class TiposContenidoIds
        {
            public const int GuiaPractica = 1;
            public const int Video = 2;
            public const int Infografia = 3;
            public const int Articulo = 4;
        }

        public static class CategoriasContenidoIds
        {
            public const int CompostajeDomestico = 1;
            public const int SeparacionDeResiduos = 2;
            public const int ImpactoAmbiental = 3;
        }

        public static class CalidadesResiduoIds
        {
            public const int Alta = 1;
            public const int Media = 2;
        }

        public static class TiposRecompensaIds
        {
            public const int Digital = 1;
            public const int Tangible = 2;
        }

        public static class NivelesIds
        {
            public const int Principiante = 1;
            public const int Aprendiz = 2;
            public const int Experto = 3;
            public const int MaestroCompostero = 4;
        }

        public static class TiposArchivoIds
        {
            public const int Imagen = 1;
            public const int Video = 2;
            public const int PDF = 3;
            public const int EnlaceExterno = 4;
        }
    }
}
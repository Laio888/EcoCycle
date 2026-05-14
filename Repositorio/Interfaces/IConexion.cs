using Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Repositorio.Interfaces
{
    public interface IConexion
    {
        string? StringConexion { get; set; }

        DbSet<Usuarios>? Usuarios { get; set; }
        DbSet<Archivos>? Archivos { get; set; }
        DbSet<PreferenciasUsuarios>? PreferenciasUsuarios { get; set; }
        DbSet<Niveles>? Niveles { get; set; }
        DbSet<TiposResiduos>? TiposResiduos { get; set; }
        DbSet<CalidadesResiduo>? CalidadesResiduo { get; set; }
        DbSet<RegistrosResiduos>? RegistrosResiduos { get; set; }
        DbSet<Recompensas>? Recompensas { get; set; }
        DbSet<TiposRecompensa>? TiposRecompensa { get; set; }
        DbSet<CanjesRecompensas>? CanjesRecompensas { get; set; }
        DbSet<PuntosHistoricos>? PuntosHistoricos { get; set; }
        DbSet<ContenidoEducativo>? ContenidoEducativo { get; set; }
        DbSet<TiposContenido>? TiposContenido { get; set; }
        DbSet<CategoriasContenido>? CategoriasContenido { get; set; }
        DbSet<UsuariosContenidoVisto>? UsuariosContenidoVisto { get; set; }
        DbSet<Notificaciones>? Notificaciones { get; set; }
        DbSet<TiposNotificacion>? TiposNotificacion { get; set; }
        DbSet<FeedbackUsuarios>? FeedbackUsuarios { get; set; }
        DbSet<TiposFeedback>? TiposFeedback { get; set; }
        DbSet<EstadosFeedback>? EstadosFeedback { get; set; }
        DbSet<TiposArchivo>? TiposArchivo { get; set; }
        DbSet<Auditorias>? Auditorias { get; set; }

        EntityEntry<T> Entry<T>(T entity) where T : class;
        int SaveChanges();
    }
}

using Dominio.Entidades;
using Repositorio.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Repositorio.Implementaciones
{
    public partial class Conexion : DbContext, IConexion
    {
        public string? StringConexion { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(this.StringConexion!, p => { });
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
        }

        public DbSet<Usuarios>? Usuarios { get; set; }
        public DbSet<Archivos>? Archivos { get; set; }
        public DbSet<PreferenciasUsuarios>? PreferenciasUsuarios { get; set; }
        public DbSet<Niveles>? Niveles { get; set; }
        public DbSet<TiposResiduos>? TiposResiduos { get; set; }
        public DbSet<CalidadesResiduo>? CalidadesResiduo { get; set; }
        public DbSet<RegistrosResiduos>? RegistrosResiduos { get; set; }
        public DbSet<Recompensas>? Recompensas { get; set; }
        public DbSet<TiposRecompensa>? TiposRecompensa { get; set; }
        public DbSet<CanjesRecompensas>? CanjesRecompensas { get; set; }
        public DbSet<PuntosHistoricos>? PuntosHistoricos { get; set; }
        public DbSet<ContenidoEducativo>? ContenidoEducativo { get; set; }
        public DbSet<TiposContenido>? TiposContenido { get; set; }
        public DbSet<CategoriasContenido>? CategoriasContenido { get; set; }
        public DbSet<UsuariosContenidoVisto>? UsuariosContenidoVisto { get; set; }
        public DbSet<Notificaciones>? Notificaciones { get; set; }
        public DbSet<TiposNotificacion>? TiposNotificacion { get; set; }
        public DbSet<FeedbackUsuarios>? FeedbackUsuarios { get; set; }
        public DbSet<TiposFeedback>? TiposFeedback { get; set; }
        public DbSet<EstadosFeedback>? EstadosFeedback { get; set; }
        public DbSet<TiposArchivo>? TiposArchivo { get; set; }
        public DbSet<Auditorias>? Auditorias { get; set; }
    }
}
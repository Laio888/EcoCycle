using Dominio.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Repositorio.Interfaces
{
    public interface IConexion
    {
        string? StringConexion { get; set; }

        DbSet<Usuarios>? Usuarios { get; set; }
        DbSet<Auditorias>? Auditorias { get; set; }

        EntityEntry<T> Entry<T>(T entity) where T : class;
        int SaveChanges();
    }
}
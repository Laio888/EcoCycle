using Dominio.Entidades;
using Repositorio.Implementaciones;
using Repositorio.Interfaces;
using Microsoft.EntityFrameworkCore;
using ut_presentacion.Nucleo;

namespace ut_presentacion.Repositorios
{
    [TestClass]
    public class UsuariosPrueba
    {
        private readonly IConexion? iConexion;
        private List<Usuarios>? lista;
        private Usuarios? entidad;
        private int entidadIdGuardado;
        
        public UsuariosPrueba()
        {
            iConexion = new Conexion();
            iConexion.StringConexion = Configuracion.ObtenerValor("StringConexion");
        }

        [TestMethod]
        public void Ejecutar()
        {
            Assert.AreEqual(true, Guardar());
            Assert.AreEqual(true, Modificar());
            Assert.AreEqual(true, Listar());
            Assert.AreEqual(true, Borrar());
        }

        [TestCleanup]
        public void Limpiar()
        {
            if (entidadIdGuardado > 0 && iConexion?.Usuarios != null)
            {
                var entidadExistente = iConexion.Usuarios.Find(entidadIdGuardado);
                if (entidadExistente != null)
                {
                    iConexion.Usuarios.Remove(entidadExistente);
                    iConexion.SaveChanges();
                }
            }
        }

        public bool Listar()
        {
            if (iConexion?.Usuarios == null)
                return false;
                
            this.lista = this.iConexion.Usuarios.ToList();
            return lista.Count > 0;
        }

        public bool Guardar()
        {
            if (iConexion?.Usuarios == null)
                return false;
                
            try
            {
                this.entidad = EntidadesNucleo.Usuarios(EntidadesNucleo.NivelesIds.Principiante);
                this.iConexion.Usuarios.Add(this.entidad);
                int resultado = this.iConexion.SaveChanges();
                
                if (resultado > 0)
                {
                    entidadIdGuardado = this.entidad.UsuarioId;
                    return true;
                }
                return false;
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Error al guardar: {ex.InnerException?.Message ?? ex.Message}");
                return false;
            }
        }

        public bool Modificar()
        {
            if (iConexion?.Usuarios == null || this.entidad == null)
                return false;
                
            try
            {
                var entidadActualizada = iConexion.Usuarios.Find(this.entidad.UsuarioId);
                if (entidadActualizada == null)
                    return false;
                
                entidadActualizada.FechaUltimoInicioSesion = DateTime.Now;
                
                var entry = iConexion.Entry(entidadActualizada);
                entry.State = EntityState.Modified;
                int resultado = iConexion.SaveChanges();
                return resultado > 0;
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Error al modificar: {ex.InnerException?.Message ?? ex.Message}");
                return false;
            }
        }

        public bool Borrar()
        {
            if (iConexion?.Usuarios == null || this.entidad == null)
                return false;
                
            try
            {
                var entidadActualizada = iConexion.Usuarios.Find(this.entidad.UsuarioId);
                if (entidadActualizada == null)
                    return false;
                
                iConexion.Usuarios.Remove(entidadActualizada);
                int resultado = iConexion.SaveChanges();
                
                if (resultado > 0)
                {
                    entidadIdGuardado = 0;
                    return true;
                }
                return false;
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Error al borrar: {ex.InnerException?.Message ?? ex.Message}");
                return false;
            }
        }
    }
}
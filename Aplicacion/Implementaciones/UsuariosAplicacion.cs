using Dominio.Entidades;
using Aplicacion.Interfaces;
using Microsoft.EntityFrameworkCore;
using Repositorio.Interfaces;

namespace Aplicacion.Implementaciones
{
    public class UsuariosAplicacion : IUsuariosAplicacion
    {
        private IConexion? IConexion = null;

        public UsuariosAplicacion(IConexion iConexion)
        {
            this.IConexion = iConexion;
        }

        public void Configurar(string StringConexion)
        {
            this.IConexion!.StringConexion = StringConexion;
        }

        public Usuarios? Guardar(Usuarios? entidad)
        {
            if (entidad == null)
                throw new Exception("Falta información");

            // Validación de usuario ya registrado por documento
            if (this.IConexion!.Usuarios!.Any(u => u.Documento == entidad.Documento))
                throw new Exception("El documento ya está registrado");

            // Validación de usuario ya registrado por correo
            if (this.IConexion!.Usuarios!.Any(u => u.Correo == entidad.Correo))
                throw new Exception("El correo ya está registrado");

            // Validación de usuario ya existente
            if (entidad.Id != 0)
                throw new Exception("El usuario ya se encuentra registrado");

            this.IConexion!.Usuarios!.Add(entidad);
            this.IConexion.SaveChanges();
            return entidad;
        }

        public Usuarios? Modificar(Usuarios? entidad)
        {
            if (entidad == null)
                throw new Exception("Falta información");
            if (entidad.Id == 0)
                throw new Exception("El usuario no existe en la base de datos");

            // Verificamos si el documento o correo ya existen, pero solo si no son del mismo usuario
            var usuarioExistente = this.IConexion!.Usuarios!.FirstOrDefault(u => u.Id == entidad.Id);
            if (usuarioExistente != null)
            {
                if (usuarioExistente.Nombre != entidad.Nombre)
                {
                    // Validación de documento único, exceptuando el mismo usuario
                    if (this.IConexion!.Usuarios!.Any(u => u.Documento == entidad.Documento && u.Id != entidad.Id))
                        throw new Exception("El documento ya está registrado por otro usuario");

                    // Validación de correo único, exceptuando el mismo usuario
                    if (this.IConexion!.Usuarios!.Any(u => u.Correo == entidad.Correo && u.Id != entidad.Id))
                        throw new Exception("El correo ya está registrado por otro usuario");
                }
            }
            else
            {
                throw new Exception("El usuario no existe en la base de datos");
            }

            var entry = this.IConexion!.Entry<Usuarios>(entidad);
            entry.State = EntityState.Modified;
            this.IConexion.SaveChanges();
            return entidad;
        }

        public Usuarios? Borrar(Usuarios? entidad)
        {
            if (entidad == null)
                throw new Exception("Falta información");
            if (entidad.Id == 0)
                throw new Exception("El usuario no existe en la base de datos");

            this.IConexion!.Usuarios!.Remove(entidad);
            this.IConexion.SaveChanges();
            return entidad;
        }

        public List<Usuarios> Listar()
        {
            return this.IConexion!.Usuarios!.Take(20).ToList();
        }

        public Usuarios? ObtenerPorId(int id)
        {
            if (id <= 0)
                throw new Exception("Id inválido");

            return this.IConexion!.Usuarios!
                                 .FirstOrDefault(u => u.Id == id);
        }

    }
}


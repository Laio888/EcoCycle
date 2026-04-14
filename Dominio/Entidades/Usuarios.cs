using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio.Entidades
{
public class Usuarios
    {
        [Key]
        public int Id { get; set; }

        [Required, StringLength(100)]
        public string Nombre { get; set; } = null!;

        [Required, StringLength(50)]
        public string Documento { get; set; } = null!;

        [StringLength(200)]
        public string? Direccion { get; set; }

        [StringLength(20)]
        public string? Telefono { get; set; }

        [Required, StringLength(150)]
        public string Correo { get; set; } = null!;

        [Required, StringLength(255)]
        public string Contraseña { get; set; } = null!;

        public DateTime? Fecha_Nacimiento { get; set; }

        public string Rol { get; set; }  // "admin" o "usuario"
    }
}

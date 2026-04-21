using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dominio.Entidades
{
    public class Auditorias
    {
        [Key]
        public int Id { get; set; }

        public string Entidad { get; set; } = string.Empty; 

        public int EntidadId { get; set; } // Id del registro afectado

        public string Accion { get; set; } = string.Empty; // Ej: "Agregar", "Actualizar", "Eliminar"

        public string? DatosAntes { get; set; } // JSON o texto con los datos antes del cambio
        public string? DatosDespues { get; set; } // JSON o texto con los datos después del cambio

        public string UsuarioAccion { get; set; } = string.Empty; // Quién hizo el cambio

        public DateTime FechaAccion { get; set; } = DateTime.Now; // Cuándo ocurrió
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio.Entidades
{
    [Table("Usuarios")]  
    public class Usuarios
    {
        [Key]
        [Column("UsuarioId")]  
        public int UsuarioId { get; set; }

        [Required]
        [StringLength(255)]
        [Column("CorreoElectronico")]  
        public string CorreoElectronico { get; set; } = null!;

        [Required]
        [StringLength(255)]
        [Column("ContrasenaHash")]  
        public string ContrasenaHash { get; set; } = null!;

        [Column("FechaRegistro")]
        public DateTime FechaRegistro { get; set; }

        [Column("FechaUltimoInicioSesion")]
        public DateTime? FechaUltimoInicioSesion { get; set; }

        [Required]
        [Column("NivelIdActual")]
        public int NivelIdActual { get; set; }

        // Propiedades de navegación
        [ForeignKey("NivelIdActual")]
        public virtual Niveles NivelActual { get; set; } = null!;
    }
}

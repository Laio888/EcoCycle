using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio.Entidades
{
    [Table("UsuariosContenidoVisto")]
    public class UsuariosContenidoVisto
    {
        [Key]
        [Column("UsuarioContenidoVistoId")]
        public int UsuarioContenidoVistoId { get; set; }

        [Required]
        [Column("UsuarioId")]
        public int UsuarioId { get; set; }

        [Required]
        [Column("ContenidoId")]
        public int ContenidoId { get; set; }

        [Column("FechaVisionado")]
        public DateTime FechaVisionado { get; set; }

        // Propiedades de navegación
        [ForeignKey("UsuarioId")]
        public virtual Usuarios Usuario { get; set; } = null!;

        [ForeignKey("ContenidoId")]
        public virtual ContenidoEducativo Contenido { get; set; } = null!;
    }
}
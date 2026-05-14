using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio.Entidades
{
    [Table("PreferenciasUsuarios")] 
    public class PreferenciasUsuarios
    {
        [Key]
        [Column("PreferenciaUsuarioId")]
        public int PreferenciaUsuarioId { get; set; }

        [Required]
        [Column("UsuarioId")]
        public int UsuarioId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("Clave")]
        public string Clave { get; set; } = null!;

        [Required]
        [StringLength(500)]
        [Column("Valor")]
        public string Valor { get; set; } = null!;

        [Column("FechaActualizacion")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]  // AGREGADO
        public DateTime FechaActualizacion { get; set; }

        // Propiedades de navegación
        [ForeignKey("UsuarioId")]
        public virtual Usuarios Usuario { get; set; } = null!;
    }
}
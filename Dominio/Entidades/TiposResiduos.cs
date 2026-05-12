using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio.Entidades
{
    [Table("TiposResiduos")]
    public class TiposResiduos
    {
        [Key]
        [Column("TipoResiduoId")]
        public int TipoResiduoId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("Nombre")]
        public string Nombre { get; set; } = null!;

        [Required]
        [Column("CalidadResiduoId")]
        public int CalidadResiduoId { get; set; }

        [StringLength(500)]
        [Column("AporteNutricional")]
        public string? AporteNutricional { get; set; }

        [Column("RelacionCarbono")]
        public int? RelacionCarbono { get; set; }

        [Column("RelacionNitrogeno")]
        public int? RelacionNitrogeno { get; set; }

        // Propiedades de navegación
        [ForeignKey("CalidadResiduoId")]
        public virtual CalidadesResiduo CalidadResiduo { get; set; } = null!;

        // Colección de navegación inversa (un tipo de residuo puede tener muchos registros)
        public virtual ICollection<RegistrosResiduos> RegistrosResiduos { get; set; } = new List<RegistrosResiduos>();
    }
}
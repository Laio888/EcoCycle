using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio.Entidades
{
    [Table("CalidadesResiduo")]
    public class CalidadesResiduo
    {
        [Key]
        [Column("CalidadResiduoId")]
        public int CalidadResiduoId { get; set; }

        [Required]
        [StringLength(50)]
        [Column("Nombre")]
        public string Nombre { get; set; } = null!;

        [Required]
        [Column("FactorBase")]
        public decimal FactorBase { get; set; }  // DECIMAL(5,2) en BD

        // Colección de navegación inversa
        public virtual ICollection<TiposResiduos> TiposResiduos { get; set; } = new List<TiposResiduos>();
    }
}
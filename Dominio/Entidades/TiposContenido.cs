using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio.Entidades
{
    [Table("TiposContenido")]
    public class TiposContenido
    {
        [Key]
        [Column("TipoContenidoId")]
        public int TipoContenidoId { get; set; }

        [Required]
        [StringLength(50)]
        [Column("Nombre")]
        public string Nombre { get; set; } = null!;

        // Colección de navegación inversa
        public virtual ICollection<ContenidoEducativo> ContenidosEducativos { get; set; } = new List<ContenidoEducativo>();
    }
}
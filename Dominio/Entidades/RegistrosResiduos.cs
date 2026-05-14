using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio.Entidades
{
    [Table("RegistrosResiduos")]
    public class RegistrosResiduos
    {
        [Key]
        [Column("RegistroResiduoId")]
        public int RegistroResiduoId { get; set; }

        [Required]
        [Column("UsuarioId")]
        public int UsuarioId { get; set; }

        [Required]
        [Column("TipoResiduoId")]
        public int TipoResiduoId { get; set; }

        [Required]
        [Column("PesoKg")]
        public decimal PesoKg { get; set; }

        [Column("FechaRegistro")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]  // AGREGADO
        public DateTime FechaRegistro { get; set; }

        [Column("EvidenciaArchivoId")]
        public int? EvidenciaArchivoId { get; set; }

        // Propiedades de navegación
        [ForeignKey("UsuarioId")]
        public virtual Usuarios Usuario { get; set; } = null!;

        [ForeignKey("TipoResiduoId")]
        public virtual TiposResiduos TipoResiduo { get; set; } = null!;

        [ForeignKey("EvidenciaArchivoId")]
        public virtual Archivos EvidenciaArchivo { get; set; } = null!;

        public virtual ICollection<PuntosHistoricos> PuntosHistoricos { get; set; } = new List<PuntosHistoricos>();
    }
}
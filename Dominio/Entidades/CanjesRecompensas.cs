using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio.Entidades
{
    [Table("CanjesRecompensas")]
    public class CanjesRecompensas
    {
        [Key]
        [Column("CanjeId")]
        public int CanjeId { get; set; }

        [Required]
        [Column("UsuarioId")]
        public int UsuarioId { get; set; }

        [Required]
        [Column("RecompensaId")]
        public int RecompensaId { get; set; }

        [Column("FechaCanje")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)] // AGREGADO: Indica que la BD genera el valor
        public DateTime FechaCanje { get; set; }

        [Required]
        [Column("PuntosGastados")]
        public int PuntosGastados { get; set; }

        [Column("ComprobanteArchivoId")]
        public int? ComprobanteArchivoId { get; set; }

        // Propiedades de navegación
        [ForeignKey("UsuarioId")]
        public virtual Usuarios Usuario { get; set; } = null!;

        [ForeignKey("RecompensaId")]
        public virtual Recompensas Recompensa { get; set; } = null!;

        [ForeignKey("ComprobanteArchivoId")]
        public virtual Archivos ComprobanteArchivo { get; set; } = null!;

        // Colección de navegación inversa
        public virtual ICollection<PuntosHistoricos> PuntosHistoricos { get; set; } = new List<PuntosHistoricos>();
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio.Entidades
{
    [Table("PuntosHistoricos")]
    public class PuntosHistoricos
    {
        [Key]
        [Column("PuntoHistoricoId")]
        public int PuntoHistoricoId { get; set; }

        [Required]
        [Column("UsuarioId")]
        public int UsuarioId { get; set; }

        [Column("FechaCambio")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]  // AGREGADO
        public DateTime FechaCambio { get; set; }

        [Required]
        [Column("PuntosAcumulados")]
        public int PuntosAcumulados { get; set; }

        [Required]
        [StringLength(255)]
        [Column("Motivo")]
        public string Motivo { get; set; } = null!;

        [Column("RegistroResiduoOrigenId")]
        public int? RegistroResiduoOrigenId { get; set; }

        [Column("CanjeOrigenId")]
        public int? CanjeOrigenId { get; set; }

        // Propiedades de navegación
        [ForeignKey("UsuarioId")]
        public virtual Usuarios Usuario { get; set; } = null!;

        [ForeignKey("RegistroResiduoOrigenId")]
        public virtual RegistrosResiduos RegistroResiduoOrigen { get; set; } = null!;

        [ForeignKey("CanjeOrigenId")]
        public virtual CanjesRecompensas CanjeOrigen { get; set; } = null!;
    }
}
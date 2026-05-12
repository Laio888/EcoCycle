using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio.Entidades
{
    [Table("Recompensas")]
    public class Recompensas
    {
        [Key]
        [Column("RecompensaId")]
        public int RecompensaId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("Nombre")]
        public string Nombre { get; set; } = null!;

        [StringLength(500)]
        [Column("Descripcion")]
        public string? Descripcion { get; set; }

        [Required]
        [Column("TipoRecompensaId")]
        public int TipoRecompensaId { get; set; }

        [Required]
        [Column("CostoPuntos")]
        public int CostoPuntos { get; set; }

        [Column("StockDisponible")]
        public int? StockDisponible { get; set; }

        [Column("EsIlimitado")]
        public bool EsIlimitado { get; set; }

        [Column("FechaVigenciaDesde")]
        public DateTime? FechaVigenciaDesde { get; set; }

        [Column("FechaVigenciaHasta")]
        public DateTime? FechaVigenciaHasta { get; set; }

        [Column("ImagenArchivoId")]
        public int? ImagenArchivoId { get; set; }

        // Propiedades de navegación
        [ForeignKey("TipoRecompensaId")]
        public virtual TiposRecompensa TipoRecompensa { get; set; } = null!;

        [ForeignKey("ImagenArchivoId")]
        public virtual Archivos ImagenArchivo { get; set; } = null!;

        // Colección de navegación inversa
        public virtual ICollection<CanjesRecompensas> CanjesRecompensas { get; set; } = new List<CanjesRecompensas>();
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio.Entidades
{
    [Table("Archivos")]
    public class Archivos
    {
        [Key]
        [Column("ArchivoId")]
        public int ArchivoId { get; set; }

        [Required]
        [StringLength(500)]
        [Column("Url")]
        public string Url { get; set; } = null!;

        [Column("TipoArchivoId")]
        public int TipoArchivoId { get; set; }

        [Column("EsExterno")]
        public bool EsExterno { get; set; }

        [StringLength(100)]
        [Column("Proveedor")]
        public string? Proveedor { get; set; }

        [StringLength(200)]
        [Column("Descripcion")]
        public string? Descripcion { get; set; }

        [Column("FechaCreacion")]
        public DateTime FechaCreacion { get; set; }
    }
}
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dominio.Entidades
{
    [Table("TiposArchivo")]
    public class TiposArchivo
    {
        [Key]
        [Column("TipoArchivoId")]
        public int TipoArchivoId { get; set; }

        [Required]
        [StringLength(50)]
        [Column("Nombre")]
        public string Nombre { get; set; } = null!;

        // Colección de navegación inversa
        public virtual ICollection<Archivos>? Archivos { get; set; }
    }
}
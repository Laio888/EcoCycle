using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio.Entidades
{
    [Table("CategoriasContenido")]
    public class CategoriasContenido
    {
        [Key]
        [Column("CategoriaContenidoId")]
        public int CategoriaContenidoId { get; set; }

        [Required]
        [StringLength(100)]
        [Column("Nombre")]
        public string Nombre { get; set; } = null!;

        // Colección de navegación inversa
        public virtual ICollection<ContenidoEducativo> ContenidosEducativos { get; set; } = new List<ContenidoEducativo>();
    }
}
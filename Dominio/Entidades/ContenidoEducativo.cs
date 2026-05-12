using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio.Entidades
{
    [Table("ContenidoEducativo")]
    public class ContenidoEducativo
    {
        [Key]
        [Column("ContenidoId")]
        public int ContenidoId { get; set; }

        [Required]
        [StringLength(200)]
        [Column("Titulo")]
        public string Titulo { get; set; } = null!;

        [Required]
        [Column("TipoContenidoId")]
        public int TipoContenidoId { get; set; }

        [Required]
        [Column("CategoriaContenidoId")]
        public int CategoriaContenidoId { get; set; }

        [Column("RecursoArchivoId")]
        public int? RecursoArchivoId { get; set; }

        [Column("EsExterno")]
        public bool EsExterno { get; set; }

        [StringLength(500)]
        [Column("FuenteExterna")]
        public string? FuenteExterna { get; set; }

        [Column("FechaPublicacion")]
        public DateTime FechaPublicacion { get; set; }

        // Propiedades de navegación
        [ForeignKey("TipoContenidoId")]
        public virtual TiposContenido TipoContenido { get; set; } = null!;

        [ForeignKey("CategoriaContenidoId")]
        public virtual CategoriasContenido CategoriaContenido { get; set; } = null!;

        [ForeignKey("RecursoArchivoId")]
        public virtual Archivos RecursoArchivo { get; set; } = null!;

        // Colección de navegación inversa (un contenido puede ser visto por muchos usuarios)
        public virtual ICollection<UsuariosContenidoVisto> UsuariosContenidoVisto { get; set; } = new List<UsuariosContenidoVisto>();
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio.Entidades
{

    [Table("Niveles")]
    public class Niveles
    {
        [Key]
        [Column("NivelId")]
        public int NivelId { get; set; }

        [Required]
        [StringLength(50)]
        [Column("NombreNivel")]
        public string NombreNivel { get; set; } = null!;

        [Column("PuntosMinimoNecesario")]
        public int PuntosMinimoNecesario { get; set; }

        [Column("PuntosMaximo")]
        public int PuntosMaximo { get; set; }

        [Column("InsigniaArchivoId")]
        public int? InsigniaArchivoId { get; set; }

        [ForeignKey("InsigniaArchivoId")]
        public virtual Archivos InsigniaArchivo { get; set; } = null!;
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dominio.Entidades
{
    [Table("FeedbackUsuarios")]
    public class FeedbackUsuarios
    {
        [Key]
        [Column("FeedbackId")]
        public int FeedbackId { get; set; }

        [Required]
        [Column("UsuarioId")]
        public int UsuarioId { get; set; }

        [Required]
        [Column("TipoFeedbackId")]
        public int TipoFeedbackId { get; set; }

        [Required]
        [StringLength(1000)]
        [Column("Mensaje")]
        public string Mensaje { get; set; } = null!;

        [Column("Fecha")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]  // AGREGADO
        public DateTime Fecha { get; set; }

        [Required]
        [Column("EstadoFeedbackId")]
        public int EstadoFeedbackId { get; set; }

        // Propiedades de navegación
        [ForeignKey("UsuarioId")]
        public virtual Usuarios Usuario { get; set; } = null!;

        [ForeignKey("TipoFeedbackId")]
        public virtual TiposFeedback TipoFeedback { get; set; } = null!;

        [ForeignKey("EstadoFeedbackId")]
        public virtual EstadosFeedback EstadoFeedback { get; set; } = null!;
    }
}
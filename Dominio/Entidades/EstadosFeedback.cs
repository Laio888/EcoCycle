using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dominio.Entidades
{
    [Table("EstadosFeedback")]
    public class EstadosFeedback
    {
        [Key]
        [Column("EstadoFeedbackId")]
        public int EstadoFeedbackId { get; set; }

        [Required]
        [StringLength(50)]
        [Column("Nombre")]
        public string Nombre { get; set; } = null!;

        public virtual ICollection<FeedbackUsuarios> FeedbacksUsuarios { get; set; } = new List<FeedbackUsuarios>();
    }
}
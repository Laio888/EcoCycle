using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Dominio.Entidades
{
    [Table("TiposFeedback")]
    public class TiposFeedback
    {
        [Key]
        [Column("TipoFeedbackId")]
        public int TipoFeedbackId { get; set; }

        [Required]
        [StringLength(50)]
        [Column("Nombre")]
        public string Nombre { get; set; } = null!;

        public virtual ICollection<FeedbackUsuarios> FeedbacksUsuarios { get; set; } = new List<FeedbackUsuarios>();
    }
}
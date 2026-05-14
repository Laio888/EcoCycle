using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio.Entidades
{
    [Table("Notificaciones")]
    public class Notificaciones
    {
        [Key]
        [Column("NotificacionId")]
        public int NotificacionId { get; set; }

        [Required]
        [Column("UsuarioId")]
        public int UsuarioId { get; set; }

        [Required]
        [Column("TipoNotificacionId")]
        public int TipoNotificacionId { get; set; }

        [Required]
        [StringLength(500)]
        [Column("Mensaje")]
        public string Mensaje { get; set; } = null!;

        [Column("FechaEnvio")]
        [DatabaseGenerated(DatabaseGeneratedOption.Computed)]  // AGREGADO
        public DateTime FechaEnvio { get; set; }

        [Column("Leida")]
        public bool Leida { get; set; }

        // Propiedades de navegación
        [ForeignKey("UsuarioId")]
        public virtual Usuarios Usuario { get; set; } = null!;

        [ForeignKey("TipoNotificacionId")]
        public virtual TiposNotificacion TipoNotificacion { get; set; } = null!;
    }
}
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dominio.Entidades
{
    [Table("TiposNotificacion")]
    public class TiposNotificacion
    {
        [Key]
        [Column("TipoNotificacionId")]
        public int TipoNotificacionId { get; set; }

        [Required]
        [StringLength(50)]
        [Column("Nombre")]
        public string Nombre { get; set; } = null!;

        // Colección de navegación inversa
        public virtual ICollection<Notificaciones> Notificaciones { get; set; } = new List<Notificaciones>();
    }
}
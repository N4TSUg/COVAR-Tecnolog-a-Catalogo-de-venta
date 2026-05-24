using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace COVAR_Tecnologia.Models
{
    public class cotec_mensaje
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Texto { get; set; }
        public DateTime FechaEnvio { get; set; } = DateTime.Now;
        // Para saber si el mensaje lo escribió el Cliente (false) o el Administrador/Vendedor (true)
        public bool EsRespuestaAdmin { get; set; }
        // Relación: A qué ticket pertenece este mensaje
        [Required]
        public int TicketSoporteId { get; set; }
        [ForeignKey("TicketSoporteId")]
        public cotec_ticketSoporte TicketSoporte { get; set; }
    }
}


using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace COVAR_Tecnologia.Models
{
    public class cotec_mensaje
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string Contenido { get; set; }
        [Required]
        public DateTime Fecha { get; set; }
        [Required]
        public int TicketSoporteId { get; set; }
        [ForeignKey("TicketSoporteId")]
        public cotec_ticketSoporte Ticket { get; set; }
    }
}

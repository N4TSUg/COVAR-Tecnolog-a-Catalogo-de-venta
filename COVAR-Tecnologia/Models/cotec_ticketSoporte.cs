using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace COVAR_Tecnologia.Models
{
    public class cotec_ticketSoporte
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? UsuarioId { get; set; }

        [ForeignKey("UsuarioId")]
        public cotec_usuario Usuario { get; set; }

        [Required(ErrorMessage = "El Asunto es obligatorio"), StringLength(200, ErrorMessage = "Máximo 200 caracteres")]
        public string Asunto { get; set; }

        public bool EsComplejo { get; set; }

        [Required(ErrorMessage = "El Estado del ticket es obligatorio.")]
        public cotec_estadoTicket Estado { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.Now;

        // Relación con los mensajes del chat
        public List<cotec_mensaje> Mensajes { get; set; }
    }
}

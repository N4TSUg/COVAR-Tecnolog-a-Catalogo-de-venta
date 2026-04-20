using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace COVAR_Tecnologia.Models
{
    public class cotec_ticketSoporte
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int? UsuarioId { get; set; }
        [ForeignKey("UsuarioId")]
        public cotec_usuario Usuario { get; set; }
        [Required,StringLength(200)]
        public string Asunto { get; set; }
        public bool EsComplejo { get; set; }
        [Required]
        public cotec_estadoTicket Estado { get; set; }
        public List<cotec_mensaje> Mensajes { get; set; }

    }
}

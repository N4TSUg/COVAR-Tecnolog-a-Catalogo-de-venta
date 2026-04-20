using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace COVAR_Tecnologia.Models
{
    public class cotec_usuario
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required, EmailAddress, StringLength(150)]
        public string Email { get; set; }
        [Required, StringLength(20)]
        public string Password { get; set; }
        public List<cotec_ticketSoporte> Tickets { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace COVAR_Tecnologia.Models
{
    public class Usuario
    {
        [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required(ErrorMessage = "El Correo Electrónico es obligatorio"), EmailAddress(ErrorMessage = "Debe ser un correo válido"), StringLength(150, ErrorMessage = "Máximo 150 caracteres")]
        public string Email { get; set; }
        [Required(ErrorMessage = "La Contraseña es obligatoria"), StringLength(100, ErrorMessage = "Máximo 100 caracteres")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Debe asignar un Rol al usuario.")]
        public int? RolId { get; set; }
        [ForeignKey("RolId")]
        public Rol Rol { get; set; }
        public List<TicketSoporte> Tickets { get; set; }
    }
}

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace COVAR_Tecnologia.Models
{
    public class cotec_rol
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required, StringLength(50)]
        public string Nombre { get; set; }

        // Propiedad de navegación: Un rol tiene muchos usuarios
        public List<cotec_usuario> Usuarios { get; set; }
    }
}

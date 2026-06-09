using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace COVAR_Tecnologia.Models
{
    public class Categoria
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required(ErrorMessage = "El Nombre de la Categoría es obligatorio"), StringLength(50, ErrorMessage = "Máximo 50 caracteres")]
        public string Nombre { get; set; }
        public List<Producto> Productos { get; set; }
    }
}

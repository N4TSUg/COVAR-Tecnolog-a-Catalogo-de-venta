using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace COVAR_Tecnologia.Models
{
    public class cotec_imagenProducto
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string RutaImagen { get; set; } // Aquí va la URL local o de internet
        // Relación: Esta imagen pertenece a un producto específico
        [Required]
        public int ProductoId { get; set; }        
        [ForeignKey("ProductoId")]
        public cotec_producto Producto { get; set; }
    }
}
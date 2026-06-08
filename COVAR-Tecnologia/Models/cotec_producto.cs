using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace COVAR_Tecnologia.Models
{
    public class cotec_producto
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required(ErrorMessage = "El Nombre es obligatorio"), StringLength(100, ErrorMessage = "Máximo 100 caracteres")]
        public string Nombre { get; set; }
        [Required(ErrorMessage = "La Descripción es obligatoria"), StringLength(500, ErrorMessage = "Máximo 500 caracteres")]
        public string Descripcion { get; set; }
        [Required(ErrorMessage = "El Precio es obligatorio"), Column(TypeName="decimal(18,2)")]
        public decimal Precio { get; set; }
        public string? ImagenURL { get; set; }
        [Required(ErrorMessage = "Debe seleccionar una Marca.")]
        public int MarcaId { get; set; }
        [ForeignKey("MarcaId")]
        public cotec_marca Marca { get; set; }
        [Required(ErrorMessage = "Debe seleccionar una Categoría.")]
        public int CategoriaId { get; set; }
        [ForeignKey("CategoriaId")]
        public cotec_categoria Categoria { get; set; }
        public string GetWhatsAppLink()
        {
            // 1. Define el número de tu negocio (Sin símbolos, usa el código de país. Ej: 51 para Perú)
            string numeroVendedor = "51928876259";

            // 2. Arma el mensaje dinámico usando las propiedades de la clase
            string mensaje = $"Hola, me interesa el producto '{Nombre}' que tiene un precio de S/ {Precio.ToString("0.00")}. ¿Aún está disponible?";

            // 3. Codifica el texto para que la URL sea válida
            string mensajeCodificado = Uri.EscapeDataString(mensaje);

            // 4. Retorna el enlace final armado
            return $"https://wa.me/{numeroVendedor}?text={mensajeCodificado}";
        }

    }
}

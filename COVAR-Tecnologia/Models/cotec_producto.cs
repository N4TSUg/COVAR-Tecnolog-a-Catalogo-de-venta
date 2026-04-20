using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace COVAR_Tecnologia.Models
{
    public class cotec_producto
    {
        [Key,DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required,StringLength(100)]
        public string Nombre { get; set; }
        [Required,StringLength(500)]
        public string Descripcion { get; set; }
        [Required,Column(TypeName="decimal(18,2)")]
        public decimal Precio { get; set; }
        public string ImagenURL { get; set; }
        [Required]
        public int MarcaId { get; set; }
        [ForeignKey("MarcaId")]
        public cotec_marca Marca { get; set; }
        [Required]
        public int CategoriaId { get; set; }
        [ForeignKey("CategoriaId")]
        public cotec_categoria Categoria { get; set; }
        public string GetWhatsAppLink()
        {
            // 1. Define el número de tu negocio (Sin símbolos, usa el código de país. Ej: 51 para Perú)
            string numeroVendedor = "51928876259";

            // 2. Arma el mensaje dinámico usando las propiedades de la clase
            string mensaje = $"Hola, me interesa el producto '{Nombre}' que tiene un precio de {Precio:C}. ¿Aún está disponible?";

            // 3. Codifica el texto para que la URL sea válida
            string mensajeCodificado = Uri.EscapeDataString(mensaje);

            // 4. Retorna el enlace final armado
            return $"https://wa.me/{numeroVendedor}?text={mensajeCodificado}";
        }

    }
}

using Microsoft.AspNetCore.Mvc.Rendering;

namespace COVAR_Tecnologia.Models
{
    public class CatalogoViewModel
    {
        public IEnumerable<Producto> Productos { get; set; } = new List<Producto>();
        
        // Paginación
        public int PaginaActual { get; set; }
        public int TotalPaginas { get; set; }

        // Filtros actuales
        public string? Busqueda { get; set; }
        public int? CategoriaId { get; set; }
        public int? MarcaId { get; set; }

        // Listas para los dropdowns
        public IEnumerable<SelectListItem> Categorias { get; set; } = new List<SelectListItem>();
        public IEnumerable<SelectListItem> Marcas { get; set; } = new List<SelectListItem>();
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using COVAR_Tecnologia.Data;
using COVAR_Tecnologia.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Diagnostics;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace COVAR_Tecnologia.Controllers
{
    public class HomeController : Controller
    {
        private readonly CoTecDBContext _context;

        // Inyectamos la base de datos
        public HomeController(CoTecDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? busqueda, int? categoriaId, int? marcaId, int pagina = 1)
        {
            int pageSize = 12; // Productos por página
            
            var query = _context.Productos
                                .Include(p => p.Marca)
                                .Include(p => p.Categoria)
                                .AsQueryable();

            // Aplicar Búsqueda
            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                query = query.Where(p => p.Nombre.Contains(busqueda) || p.Descripcion.Contains(busqueda));
            }

            // Aplicar Filtros
            if (categoriaId.HasValue)
            {
                query = query.Where(p => p.CategoriaId == categoriaId);
            }
            if (marcaId.HasValue)
            {
                query = query.Where(p => p.MarcaId == marcaId);
            }

            // Paginación
            int totalItems = await query.CountAsync();
            int totalPaginas = (int)Math.Ceiling(totalItems / (double)pageSize);
            // Evitar páginas inválidas
            if (pagina < 1) pagina = 1;
            if (pagina > totalPaginas && totalPaginas > 0) pagina = totalPaginas;

            var productos = await query.OrderByDescending(p => p.Id)
                                       .Skip((pagina - 1) * pageSize)
                                       .Take(pageSize)
                                       .ToListAsync();

            // Cargar listas para selects
            var categorias = await _context.Categorias.OrderBy(c => c.Nombre).ToListAsync();
            var marcas = await _context.Marcas.OrderBy(m => m.Nombre).ToListAsync();

            // Llenar el ViewModel
            var vm = new CatalogoViewModel
            {
                Productos = productos,
                PaginaActual = pagina,
                TotalPaginas = totalPaginas,
                Busqueda = busqueda,
                CategoriaId = categoriaId,
                MarcaId = marcaId,
                Categorias = new SelectList(categorias, "Id", "Nombre", categoriaId),
                Marcas = new SelectList(marcas, "Id", "Nombre", marcaId)
            };

            return View(vm);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // GET: Home/Detalle/5
        public async Task<IActionResult> Detalle(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Buscamos el producto e incluimos su Marca y Categoría
            var producto = await _context.Productos
                                         .Include(p => p.Marca)
                                         .Include(p => p.Categoria)
                                         .FirstOrDefaultAsync(m => m.Id == id);

            if (producto == null)
            {
                return NotFound();
            }

            return View(producto);
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using COVAR_Tecnologia.Data;
using COVAR_Tecnologia.Models;
using System.Diagnostics;

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

        public async Task<IActionResult> Index()
        {
            // Traemos todos los productos e incluimos Marca y Categoría para mostrar esa info en las tarjetas
            var productos = await _context.Productos
                                          .Include(p => p.Marca)
                                          .Include(p => p.Categoria)
                                          .ToListAsync();
            return View(productos);
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
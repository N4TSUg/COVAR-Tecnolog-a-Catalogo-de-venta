using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using COVAR_Tecnologia.Data;
using COVAR_Tecnologia.Models;
using Microsoft.AspNetCore.Authorization;

namespace COVAR_Tecnologia.Controllers
{
    // Bloqueamos el acceso solo para el Administrador y vendedor
    [Authorize(Roles = "Administrador,Vendedor")]
    public class CategoriaController : Controller
    {
        private readonly CoTecDBContext _context;

        // Inyección de dependencias para acceder a la base de datos
        public CategoriaController(CoTecDBContext context)
        {
            _context = context;
        }

        // 1. LISTADO DE CATEGORÍAS (GET)
        public async Task<IActionResult> Index()
        {
            var categorias = await _context.Categorias.ToListAsync();
            return View(categorias);
        }

        // 2. CREAR - VISTA (GET)
        public IActionResult Crear()
        {
            return View();
        }

        // 3. CREAR - LÓGICA (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(cotec_categoria categoria)
        {
            ModelState.Remove("Productos");
            if (ModelState.IsValid)
            {
                _context.Add(categoria);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(categoria);
        }

        // 4. EDITAR - VISTA (GET)
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null)
            {
                return NotFound();
            }

            return View(categoria);
        }

        // 5. EDITAR - LÓGICA (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, cotec_categoria categoria)
        {
            ModelState.Remove("Productos");
            if (id != categoria.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _context.Update(categoria);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(categoria);
        }

        // 6. ELIMINAR (Lógica directa)
        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var categoria = await _context.Categorias.FindAsync(id);

            if (categoria != null)
            {
                _context.Categorias.Remove(categoria);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
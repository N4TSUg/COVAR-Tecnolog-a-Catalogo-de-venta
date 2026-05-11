using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using COVAR_Tecnologia.Data;
using COVAR_Tecnologia.Models;
using Microsoft.AspNetCore.Authorization;

namespace COVAR_Tecnologia.Controllers
{
    // Bloqueamos el acceso solo para el Administrador y vendedor
    [Authorize(Roles = "Administrador,Vendedor")]
    public class MarcaController : Controller
    {
        private readonly CoTecDBContext _context;

        // Inyección de dependencias para acceder a la base de datos
        public MarcaController(CoTecDBContext context)
        {
            _context = context;
        }

        // 1. LISTADO DE MARCAS (GET)
        public async Task<IActionResult> Index()
        {
            var marcas = await _context.Marcas.ToListAsync();
            return View(marcas);
        }

        // 2. CREAR - VISTA (GET)
        public IActionResult Crear()
        {
            return View();
        }

        // 3. CREAR - LÓGICA (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(cotec_marca marca)
        {
            ModelState.Remove("Productos");
            if (ModelState.IsValid)
            {
                _context.Add(marca);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(marca);
        }

        // 4. EDITAR - VISTA (GET)
        public async Task<IActionResult> Editar(int? id)
        {
            ModelState.Remove("Productos");
            if (id == null)
            {
                return NotFound();
            }

            var marca = await _context.Marcas.FindAsync(id);
            if (marca == null)
            {
                return NotFound();
            }

            return View(marca);
        }

        // 5. EDITAR - LÓGICA (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, cotec_marca marca)
        {
            if (id != marca.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _context.Update(marca);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(marca);
        }

        // 6. ELIMINAR (Lógica directa)
        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var marca = await _context.Marcas.FindAsync(id);

            if (marca != null)
            {
                _context.Marcas.Remove(marca);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
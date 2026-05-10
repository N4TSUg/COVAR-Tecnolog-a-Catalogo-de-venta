using COVAR_Tecnologia.Data;
using COVAR_Tecnologia.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace COVAR_Tecnologia.Controllers
{
    // Bloqueamos el acceso solo para el Administrador y vendedor
    [Authorize(Roles = "Administrador,Vendedor")]
    public class ProductoController : Controller
    {
        private readonly CoTecDBContext _context;

        public ProductoController(CoTecDBContext context)
        {
            _context = context;
        }

        // 1. LISTADO DE PRODUCTOS
        public async Task<IActionResult> Index()
        {
            // Traemos los productos incluyendo su Marca y Categoría para mostrarlas en la tabla
            var productos = await _context.Productos
                .Include(p => p.Marca)
                .Include(p => p.Categoria)
                .ToListAsync();
            return View(productos);
        }

        // 2. CREAR - VISTA (GET)
        public IActionResult Crear()
        {
            // Llenamos los DropDownLists
            ViewBag.Marcas = new SelectList(_context.Marcas, "Id", "Nombre");
            ViewBag.Categorias = new SelectList(_context.Categorias, "Id", "Nombre");
            return View();
        }

        // 3. CREAR - LÓGICA (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Puedes agregar el signo de interrogación (?) a IFormFile para indicar explícitamente que es opcional
        public async Task<IActionResult> Crear(cotec_producto producto, IFormFile? archivoImagen)
        {
            ModelState.Remove("Marca");
            ModelState.Remove("Categoria");
            ModelState.Remove("archivoImagen");
            ModelState.Remove("ImagenURL");

            if (ModelState.IsValid)
            {
                // PRIORIDAD 1: Si hay un archivo físico, lo guardamos localmente
                if (archivoImagen != null && archivoImagen.Length > 0)
                {
                    string nombreArchivo = Guid.NewGuid().ToString() + Path.GetExtension(archivoImagen.FileName);
                    string rutaCarpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/productos");

                    if (!Directory.Exists(rutaCarpeta))
                        Directory.CreateDirectory(rutaCarpeta);

                    string rutaCompleta = Path.Combine(rutaCarpeta, nombreArchivo);

                    using (var stream = new FileStream(rutaCompleta, FileMode.Create))
                    {
                        await archivoImagen.CopyToAsync(stream);
                    }

                    // Sobrescribimos el campo ImagenURL con la ruta del servidor local
                    producto.ImagenURL = "/images/productos/" + nombreArchivo;
                }
                // PRIORIDAD 2: Si no hay archivo pero puso una URL, no hacemos nada extra.
                // El "Model Binder" ya asignó el valor del input de texto a producto.ImagenURL automáticamente.

                _context.Add(producto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Si algo falla, recargamos las listas para la vista
            ViewBag.Marcas = new SelectList(_context.Marcas, "Id", "Nombre", producto.MarcaId);
            ViewBag.Categorias = new SelectList(_context.Categorias, "Id", "Nombre", producto.CategoriaId);
            return View(producto);
        }

        // 4. EDITAR - VISTA (GET)
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null) return NotFound();

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null) return NotFound();

            ViewBag.Marcas = new SelectList(_context.Marcas, "Id", "Nombre", producto.MarcaId);
            ViewBag.Categorias = new SelectList(_context.Categorias, "Id", "Nombre", producto.CategoriaId);
            return View(producto);
        }

        // 5. EDITAR - LÓGICA (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, cotec_producto producto)
        {
            ModelState.Remove("Marca");
            ModelState.Remove("Categoria");
            ModelState.Remove("archivoImagen");
            ModelState.Remove("ImagenURL");

            if (id != producto.Id) return NotFound();

            if (ModelState.IsValid)
            {
                _context.Update(producto);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewBag.Marcas = new SelectList(_context.Marcas, "Id", "Nombre", producto.MarcaId);
            ViewBag.Categorias = new SelectList(_context.Categorias, "Id", "Nombre", producto.CategoriaId);
            return View(producto);
        }

        // 6. ELIMINAR
        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null) return NotFound();

            var producto = await _context.Productos.FindAsync(id);
            if (producto != null)
            {
                _context.Productos.Remove(producto);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

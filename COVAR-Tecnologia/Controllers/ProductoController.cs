using COVAR_Tecnologia.Data;
using COVAR_Tecnologia.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace COVAR_Tecnologia.Controllers
{
    // Bloqueamos el acceso solo para el Administrador y vendedor
    [Authorize(Roles = "Administrador,Vendedor")]
    public class ProductoController : Controller
    {
        private readonly CoTecDBContext _context;
        private const string KeyName = "Nombre";
        private const string KeyId = "Id";

        public ProductoController(CoTecDBContext context)
        {
            _context = context;
        }

        // 1. LISTADO DE PRODUCTOS
        public async Task<IActionResult> Index()
        {
            var productos = await _context.Productos
                .Include(p => p.Marca)
                .Include(p => p.Categoria)
                .ToListAsync();
            return View(productos);
        }

        // 2. CREAR - VISTA (GET)
        public IActionResult Crear()
        {
            ViewBag.Marcas = new SelectList(_context.Marcas, KeyId, KeyName);
            ViewBag.Categorias = new SelectList(_context.Categorias, KeyId, KeyName);
            return View();
        }

        // 3. CREAR - LÓGICA (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Producto producto, IFormFile? archivoImagen)
        {
            ModelState.Remove("Marca");
            ModelState.Remove("Categoria");
            ModelState.Remove("archivoImagen");
            ModelState.Remove("ImagenURL");

            if (ModelState.IsValid)
            {
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

                    producto.ImagenURL = "/images/productos/" + nombreArchivo;
                }

                _context.Add(producto);
                await _context.SaveChangesAsync();
                TempData["Mensaje"] = "Producto creado exitosamente.";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Marcas = new SelectList(_context.Marcas, KeyId, KeyName, producto.MarcaId);
            ViewBag.Categorias = new SelectList(_context.Categorias, KeyId, KeyName, producto.CategoriaId);
            return View(producto);
        }

        // 4. EDITAR - VISTA (GET)
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null) return NotFound();

            var producto = await _context.Productos.FindAsync(id);
            if (producto == null) return NotFound();

            ViewBag.Marcas = new SelectList(_context.Marcas, KeyId, KeyName, producto.MarcaId);
            ViewBag.Categorias = new SelectList(_context.Categorias, KeyId, KeyName, producto.CategoriaId);
            return View(producto);
        }

        // 5. EDITAR - LÓGICA (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Producto producto, IFormFile? archivoImagen)
        {
            ModelState.Remove("Marca");
            ModelState.Remove("Categoria");
            ModelState.Remove("archivoImagen");
            ModelState.Remove("ImagenURL");

            // 1. Cláusulas de guarda tempranas
            if (id != producto.Id) return NotFound();

            if (!ModelState.IsValid)
            {
                // Si no es válido, preparamos los ViewBags y retornamos la vista
                ViewBag.Marcas = new SelectList(_context.Marcas, KeyId, KeyName, producto.MarcaId);
                ViewBag.Categorias = new SelectList(_context.Categorias, KeyId, KeyName, producto.CategoriaId);
                return View(producto);
            }

            // 2. Lógica principal (ahora sin tanta anidación)
            try
            {
                // Delegamos la complejidad de la imagen a un método externo
                producto.ImagenURL = await ProcesarImagenProducto(producto.Id, producto.ImagenURL, archivoImagen);

                _context.Update(producto);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductoExists(producto.Id)) return NotFound();
                throw; // Se omite el 'else' innecesario
            }

            TempData["Mensaje"] = "Producto actualizado exitosamente.";
            return RedirectToAction(nameof(Index));
        }

        // 3. Nuevo método privado exclusivo para manejar la imagen
        private async Task<string> ProcesarImagenProducto(int productoId, string imagenUrlActual, IFormFile? archivoImagen)
        {
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

                return "/images/productos/" + nombreArchivo;
            }

            // Si no se subió una nueva imagen y la actual está vacía, recuperamos la anterior
            if (string.IsNullOrEmpty(imagenUrlActual))
            {
                var productoExistente = await _context.Productos.AsNoTracking().FirstOrDefaultAsync(p => p.Id == productoId);
                return productoExistente?.ImagenURL;
            }

            // Si no se subió nada pero ya había una URL, conservamos la que venía en el modelo
            return imagenUrlActual;
        }

        private bool ProductoExists(int id)
        {
            return _context.Productos.Any(e => e.Id == id);
        }

        // 7. ELIMINAR - LÓGICA (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto != null)
            {
                try
                {
                    _context.Productos.Remove(producto);
                    await _context.SaveChangesAsync();
                    TempData["Mensaje"] = "Producto eliminado exitosamente.";
                }
                catch (DbUpdateException)
                {
                    TempData["Error"] = "No se puede eliminar este producto porque está referenciado en otro lugar.";
                    return RedirectToAction(nameof(Index));
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}

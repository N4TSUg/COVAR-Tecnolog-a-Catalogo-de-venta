using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using COVAR_Tecnologia.Data;
using COVAR_Tecnologia.Models;
using Microsoft.AspNetCore.Authorization;
using BCrypt.Net;

namespace COVAR_Tecnologia.Controllers
{
    [Authorize(Roles = "Administrador")] // Solo Enrique puede crear vendedores
    public class UsuarioController : Controller
    {
        private readonly CoTecDBContext _context;

        public UsuarioController(CoTecDBContext context)
        {
            _context = context;
        }

        // Listado de usuarios (Vendedores y otros)
        public async Task<IActionResult> Index()
        {
            var usuarios = await _context.Usuarios.Include(u => u.Rol).ToListAsync();
            return View(usuarios);
        }

        // Vista para crear un nuevo vendedor
        public IActionResult Crear()
        {
            // Filtramos para que solo se pueda asignar el rol de Vendedor o Cliente
            // (Administrador solo hay uno por ahora)
            ViewBag.Roles = new SelectList(_context.Roles.Where(r => r.Nombre != "Administrador"), "Id", "Nombre");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(cotec_usuario usuario, string claveRaw)
        {
            // 1. IMPORTANTE: Eliminamos la validación de 'Password' del ModelState 
            // porque lo vamos a llenar manualmente con el hash después.
            ModelState.Remove("Password");
            ModelState.Remove("Rol");
            ModelState.Remove("Tickets");

            if (ModelState.IsValid)
            {
                try
                {
                    // 2. Hasheamos la clave que viene del input 'claveRaw'
                    usuario.Password = BCrypt.Net.BCrypt.HashPassword(claveRaw);

                    _context.Add(usuario);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    // Si hay un error de base de datos (ej. correo duplicado), lo atrapamos aquí
                    ModelState.AddModelError("", "No se pudo guardar: " + ex.Message);
                }
            }

            // Si llegamos aquí, algo falló. Recargamos la lista de roles.
            ViewBag.Roles = new SelectList(_context.Roles.Where(r => r.Nombre != "Administrador"), "Id", "Nombre");
            return View(usuario);
        }

        // Método para mostrar la confirmación de eliminación (GET)
        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null) return NotFound();

            var usuario = await _context.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Id == id);
            if (usuario == null) return NotFound();

            return View(usuario);
        }

        // Método para eliminar un acceso (POST)
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario != null && usuario.Email != "enriquearana1402@gmail.com") // Protegemos al admin
            {
                try
                {
                    _context.Usuarios.Remove(usuario);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    TempData["Error"] = "No se puede eliminar este usuario porque tiene tickets de soporte u otros registros asociados.";
                    return RedirectToAction(nameof(Index));
                }
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
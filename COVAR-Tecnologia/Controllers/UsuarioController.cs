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
        private const string KeyAdmin = "Administrador";
        private const string KeyNombre = "Nombre";
        private const string KeyId = "Id";

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
            ViewBag.Roles = new SelectList(_context.Roles.Where(r => r.Nombre != KeyAdmin), KeyId, KeyNombre);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(Usuario usuario, string claveRaw)
        {
            ModelState.Remove("Password");
            ModelState.Remove("Rol");
            ModelState.Remove("Tickets");

            if (ModelState.IsValid)
            {
                try
                {
                    usuario.Password = BCrypt.Net.BCrypt.HashPassword(claveRaw);

                    _context.Add(usuario);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "No se pudo guardar: " + ex.Message);
                }
            }

            ViewBag.Roles = new SelectList(_context.Roles.Where(r => r.Nombre != KeyAdmin), KeyId, KeyNombre);
            return View(usuario);
        }

        // GET: Usuario/Editar/5
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null) return NotFound();

            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            ViewBag.Roles = new SelectList(_context.Roles.Where(r => r.Nombre != KeyAdmin), KeyId, KeyNombre, usuario.RolId);
            return View(usuario);
        }

        // POST: Usuario/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, Usuario usuario, string? claveRaw)
        {
            if (id != usuario.Id) return NotFound();

            ModelState.Remove("Password");
            ModelState.Remove("Rol");
            ModelState.Remove("Tickets");

            if (ModelState.IsValid)
            {
                try
                {
                    var userInDb = await _context.Usuarios.FindAsync(id);
                    if (userInDb == null) return NotFound();

                    userInDb.Email = usuario.Email;
                    userInDb.RolId = usuario.RolId;

                    // Solo actualizar contraseña si se proporcionó una nueva
                    if (!string.IsNullOrWhiteSpace(claveRaw))
                    {
                        userInDb.Password = BCrypt.Net.BCrypt.HashPassword(claveRaw);
                    }

                    _context.Update(userInDb);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UsuarioExists(usuario.Id)) return NotFound();
                    else throw;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error al editar: " + ex.Message);
                }
            }

            ViewBag.Roles = new SelectList(_context.Roles.Where(r => r.Nombre != KeyAdmin), KeyId, KeyNombre, usuario.RolId);
            return View(usuario);
        }

        // Método para eliminar un acceso (POST)
        [HttpPost]
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
                }
            }
            return RedirectToAction(nameof(Index));
        }

        private bool UsuarioExists(int id)
        {
            return _context.Usuarios.Any(e => e.Id == id);
        }
    }
}

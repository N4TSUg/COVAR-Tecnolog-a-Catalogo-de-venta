using COVAR_Tecnologia.Data;
using COVAR_Tecnologia.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace COVAR_Tecnologia.Controllers
{
    public class AccesoController : Controller
    {
        private readonly CoTecDBContext _context;

        // Inyectamos la base de datos
        public AccesoController(CoTecDBContext context)
        {
            _context = context;
        }

        // Método para mostrar la pantalla de Login
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // Método que se ejecuta al darle clic al botón "Ingresar"
        [HttpPost]
        public async Task<IActionResult> Login(string correo, string clave)
        {
            // 1. Buscamos al usuario por su correo e incluimos su Rol
            var usuario = await _context.Usuarios
                                         .Include(u => u.Rol)
                                         .FirstOrDefaultAsync(u => u.Email == correo);

            // 2. Verificamos si existe y si la contraseña coincide con el Hash
            if (usuario == null || !BCrypt.Net.BCrypt.Verify(clave, usuario.Password))
            {
                ViewData["Mensaje"] = "Correo o contraseña incorrectos.";
                return View();
            }

            // 3. Creamos las credenciales (Claims)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Role, usuario.Rol.Nombre)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            // 4. Creamos la cookie de sesión
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

            // 5. REDIRECCIÓN DINÁMICA POR ROL
            // Evaluamos el nombre del rol (asegúrate que coincidan con los nombres en tu DB)
            switch (usuario.Rol.Nombre)
            {
                case "Administrador":
                    // Redirige al Index del AdminController que creamos hace un momento
                    return RedirectToAction("Index", "Admin");

                case "Vendedor":
                    // Aquí podrías redirigir a un VendedorController (si decides crearlo)
                    // Por ahora lo mandaremos al Home o al inventario
                    return RedirectToAction("Index", "Vendedor");

                case "Cliente":
                    // El cliente debe ir directamente a ver el catálogo de productos
                    return RedirectToAction("Index", "Home");

                default:
                    // Por si ocurre algo inesperado, lo mandamos al Home
                    return RedirectToAction("Index", "Home");
            }
        }

        // VISTA DE REGISTRO (GET)
        public IActionResult Registrarse()
        {
            return View();
        }

        // LÓGICA DE REGISTRO (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrarse(cotec_usuario usuario, string claveRaw, string confirmarClave)
        {
            // Validaciones manuales básicas
            if (claveRaw != confirmarClave)
            {
                ViewData["Mensaje"] = "Las contraseñas no coinciden.";
                return View();
            }

            // Quitamos validaciones de objetos de navegación
            ModelState.Remove("Rol");
            ModelState.Remove("Tickets");
            ModelState.Remove("Password");

            if (ModelState.IsValid)
            {
                // Verificamos si el correo ya existe
                var existe = await _context.Usuarios.AnyAsync(u => u.Email == usuario.Email);
                if (existe)
                {
                    ViewData["Mensaje"] = "Este correo ya está registrado.";
                    return View();
                }

                // Buscamos el ID del rol "Cliente"
                var rolCliente = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == "Cliente");
                if (rolCliente == null)
                {
                    ViewData["Mensaje"] = "Error interno: El rol Cliente no existe.";
                    return View();
                }

                // Asignamos datos finales
                usuario.RolId = rolCliente.Id;
                usuario.Password = BCrypt.Net.BCrypt.HashPassword(claveRaw);

                _context.Add(usuario);
                await _context.SaveChangesAsync();

                // Redirigimos al Login para que inicie sesión
                return RedirectToAction("Login", "Acceso");
            }

            return View(usuario);
        }

        // Método para cerrar sesión
        public async Task<IActionResult> Salir()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Acceso");
        }
    }
}

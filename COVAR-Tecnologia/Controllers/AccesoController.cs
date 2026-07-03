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
        private const string KeyMensaje = "Mensaje";
        private const string ActionIndex = "Index";

        // Inyectamos la base de datos
        public AccesoController(CoTecDBContext context)
        {
            _context = context;
        }

        // Método para mostrar la pantalla de Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // Método que se ejecuta al darle clic al botón "Ingresar"
        [HttpPost]
        public async Task<IActionResult> Login(string correo, string clave, string? returnUrl = null)
        {
            // 1. Buscamos al usuario por su correo e incluimos su Rol
            var usuario = await _context.Usuarios
                                         .Include(u => u.Rol)
                                         .FirstOrDefaultAsync(u => u.Email == correo);

            // 2. Verificamos si existe y si la contraseña coincide con el Hash
            if (usuario == null || !BCrypt.Net.BCrypt.Verify(clave, usuario.Password))
            {
                ViewData[KeyMensaje] = "Correo o contraseña incorrectos.";
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

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            // 5. REDIRECCIÓN DINÁMICA POR ROL
            // Evaluamos el nombre del rol (asegúrate que coincidan con los nombres en tu DB)
            switch (usuario.Rol.Nombre)
            {
                case "Administrador":
                    // Redirige al Index del AdminController que creamos hace un momento
                    return RedirectToAction(ActionIndex, "Admin");

                case "Vendedor":
                    // Aquí podrías redirigir a un VendedorController (si decides crearlo)
                    // Por ahora lo mandaremos al Home o al inventario
                    return RedirectToAction(ActionIndex, "Vendedor");

                case "Cliente":
                    // El cliente debe ir directamente a ver el catálogo de productos
                    return RedirectToAction(ActionIndex, "Home");   
                default:
                    // Por si ocurre algo inesperado, lo mandamos al Home
                    return RedirectToAction(ActionIndex, "Home");
            }
        }

        // VISTA DE REGISTRO (GET)
        public IActionResult Registrarse(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // LÓGICA DE REGISTRO (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Registrarse(Usuario usuario, string claveRaw, string confirmarClave, string? returnUrl = null)
        {
            // Validaciones manuales básicas
            if (claveRaw != confirmarClave)
            {
                ViewData[KeyMensaje] = "Las contraseñas no coinciden.";
                return View();
            }

            // Quitamos validaciones de objetos de navegación
            ModelState.Remove("Rol");
            ModelState.Remove("RolId");
            ModelState.Remove("Tickets");
            ModelState.Remove("Password");

            if (ModelState.IsValid)
            {
                // Verificamos si el correo ya existe
                var existe = await _context.Usuarios.AnyAsync(u => u.Email == usuario.Email);
                if (existe)
                {
                    ViewData[KeyMensaje] = "Este correo ya está registrado.";
                    return View();
                }

                // Buscamos el ID del rol "Cliente"
                var rolCliente = await _context.Roles.FirstOrDefaultAsync(r => r.Nombre == "Cliente");
                if (rolCliente == null)
                {
                    ViewData[KeyMensaje] = "Error interno: El rol Cliente no existe.";
                    return View();
                }

                // Asignamos datos finales
                usuario.RolId = rolCliente.Id;
                usuario.Password = BCrypt.Net.BCrypt.HashPassword(claveRaw);

                _context.Add(usuario);
                await _context.SaveChangesAsync();

                // Auto-login después del registro
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                    new Claim(ClaimTypes.Email, usuario.Email),
                    new Claim(ClaimTypes.Role, rolCliente.Nombre)
                };

                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(claimsIdentity));

                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }

                return RedirectToAction("Index", "Home");
            }

            ViewData["ReturnUrl"] = returnUrl;
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

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using COVAR_Tecnologia.Data;
using COVAR_Tecnologia.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace COVAR_Tecnologia.Controllers
{
    // Solo clientes pueden acceder a esta sección
    [Authorize(Roles = "Cliente")]
    public class SoporteController : Controller
    {
        private readonly CoTecDBContext _context;

        public SoporteController(CoTecDBContext context)
        {
            _context = context;
        }

        // Método auxiliar para obtener el ID del usuario conectado
        private int ObtenerUsuarioId()
        {
            return int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        // 1. MIS CONSULTAS (Lista de tickets del cliente)
        public async Task<IActionResult> Index()
        {
            int usuarioId = ObtenerUsuarioId();
            var misTickets = await _context.TicketsSoporte
                                           .Include(t => t.Producto)
                                           .Where(t => t.UsuarioId == usuarioId)
                                           .OrderByDescending(t => t.FechaCreacion)
                                           .ToListAsync();
            return View(misTickets);
        }

        // 2. CREAR TICKET - VISTA (GET)
        public IActionResult Crear()
        {
            ViewBag.Productos = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Productos, "Id", "Nombre");
            return View();
        }

        // 3. CREAR TICKET - LÓGICA (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(string asunto, string mensajeInicial, int? productoId)
        {
            if (!string.IsNullOrWhiteSpace(asunto) && !string.IsNullOrWhiteSpace(mensajeInicial))
            {
                var nuevoTicket = new TicketSoporte
                {
                    UsuarioId = ObtenerUsuarioId(),
                    ProductoId = productoId,
                    Asunto = asunto,
                    Estado = EstadoTicket.Abierto,
                    FechaCreacion = DateTime.Now,
                    EsComplejo = false, // Por defecto
                    // Creamos el primer mensaje automáticamente
                    Mensajes = new List<Mensaje>
                    {
                        new Mensaje
                        {
                            Texto = mensajeInicial,
                            FechaEnvio = DateTime.Now,
                            EsRespuestaAdmin = false // Es falso porque lo escribe el cliente
                        }
                    }
                };

                _context.Add(nuevoTicket);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Productos = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(_context.Productos, "Id", "Nombre");
            ViewData["Mensaje"] = "Por favor completa todos los campos obligatorios.";
            return View();
        }

        // 3.5. CONSULTA RÁPIDA (Desde Detalle de Producto)
        [HttpGet]
        public async Task<IActionResult> ConsultaRapida(int productoId)
        {
            var producto = await _context.Productos.FindAsync(productoId);
            if (producto == null) return NotFound();

            var nuevoTicket = new TicketSoporte
            {
                UsuarioId = ObtenerUsuarioId(),
                ProductoId = productoId,
                Asunto = $"Tengo una duda acerca del {producto.Nombre}",
                Estado = EstadoTicket.Abierto,
                FechaCreacion = DateTime.Now,
                EsComplejo = false,
                Mensajes = new List<Mensaje>
                {
                    new Mensaje
                    {
                        Texto = $"Hola, quisiera saber más sobre {producto.Nombre}.",
                        FechaEnvio = DateTime.Now,
                        EsRespuestaAdmin = false
                    }
                }
            };

            _context.Add(nuevoTicket);
            await _context.SaveChangesAsync();

            // Redirigir directamente al chat de este nuevo ticket
            return RedirectToAction(nameof(Chat), new { id = nuevoTicket.Id });
        }

        // 4. VER CHAT DEL TICKET (GET)
        public async Task<IActionResult> Chat(int id)
        {
            int usuarioId = ObtenerUsuarioId();

            // Buscamos el ticket, pero validando que le pertenezca a este usuario
            var ticket = await _context.TicketsSoporte
                                       .Include(t => t.Producto)
                                       .Include(t => t.Mensajes)
                                       .FirstOrDefaultAsync(t => t.Id == id && t.UsuarioId == usuarioId);

            if (ticket == null) return NotFound();

            return View(ticket);
        }

        // 5. ENVIAR MENSAJE AL CHAT (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Chat(int id, string textoMensaje)
        {
            int usuarioId = ObtenerUsuarioId();
            var ticket = await _context.TicketsSoporte.FirstOrDefaultAsync(t => t.Id == id && t.UsuarioId == usuarioId);

            if (ticket == null) return NotFound();

            // Si el ticket está abierto y el mensaje no está vacío, lo guardamos
            if (!string.IsNullOrWhiteSpace(textoMensaje) && ticket.Estado == EstadoTicket.Abierto)
            {
                var nuevoMensaje = new Mensaje
                {
                    TicketSoporteId = id,
                    Texto = textoMensaje,
                    FechaEnvio = DateTime.Now,
                    EsRespuestaAdmin = false // Lo envía el cliente
                };

                _context.Add(nuevoMensaje);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Chat), new { id = id });
        }
    }
}
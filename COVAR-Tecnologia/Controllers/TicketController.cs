using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using COVAR_Tecnologia.Data;
using COVAR_Tecnologia.Models;
using Microsoft.AspNetCore.Authorization;

namespace COVAR_Tecnologia.Controllers
{
    [Authorize(Roles = "Administrador,Vendedor")]
    public class TicketController : Controller
    {
        private readonly CoTecDBContext _context;

        public TicketController(CoTecDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var tickets = await _context.TicketsSoporte
                                        .Include(t => t.Usuario)
                                        .OrderByDescending(t => t.FechaCreacion)
                                        .ToListAsync();
            return View(tickets);
        }

        public async Task<IActionResult> Responder(int? id)
        {
            if (id == null) return NotFound();

            var ticket = await _context.TicketsSoporte
                                       .Include(t => t.Usuario)
                                       .Include(t => t.Mensajes)
                                       .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null) return NotFound();

            return View(ticket);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Responder(int id, string textoMensaje, bool cerrarTicket)
        {
            var ticket = await _context.TicketsSoporte.FindAsync(id);
            if (ticket == null) return NotFound();

            if (!string.IsNullOrWhiteSpace(textoMensaje))
            {
                var nuevoMensaje = new cotec_mensaje
                {
                    TicketSoporteId = id,
                    Texto = textoMensaje,
                    FechaEnvio = DateTime.Now,
                    EsRespuestaAdmin = true
                };

                _context.Add(nuevoMensaje);
            }

            if (cerrarTicket)
            {
                ticket.Estado = cotec_estadoTicket.Cerrado;
                _context.Update(ticket);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("Responder", new { id = id });
        }
    }
}
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using COVAR_Tecnologia.Data;
using Microsoft.AspNetCore.Authorization;
using COVAR_Tecnologia.Models;

namespace COVAR_Tecnologia.Controllers
{
    // Solo los usuarios con rol "Vendedor" entran aquí
    [Authorize(Roles = "Vendedor")]
    public class VendedorController : Controller
    {
        private readonly CoTecDBContext _context;

        public VendedorController(CoTecDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Métricas enfocadas en el trabajo diario del vendedor
            ViewBag.TotalProductos = await _context.Productos.CountAsync();

            // Suponiendo que tu enum tiene un estado 'Abierto' o 'Pendiente'
            ViewBag.TicketsPendientes = await _context.TicketsSoporte.CountAsync(t => t.Estado == EstadoTicket.Abierto);

            return View();
        }
    }
}
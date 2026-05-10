using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using COVAR_Tecnologia.Data;
using Microsoft.AspNetCore.Authorization;
using COVAR_Tecnologia.Models;

namespace COVAR_Tecnologia.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminController : Controller
    {
        private readonly CoTecDBContext _context;

        public AdminController(CoTecDBContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Contamos los elementos para mostrarlos en el Dashboard
            ViewBag.TotalProductos = await _context.Productos.CountAsync();
            ViewBag.TotalMarcas = await _context.Marcas.CountAsync();
            ViewBag.TotalCategorias = await _context.Categorias.CountAsync();
            ViewBag.TicketsAbiertos = await _context.TicketsSoporte.CountAsync(t => t.Estado == cotec_estadoTicket.Abierto);

            return View();
        }
    }
}
using Microsoft.AspNetCore.SignalR;
using COVAR_Tecnologia.Data;
using COVAR_Tecnologia.Models;
using System.Security.Claims;
using System.Threading.Tasks;
using System;

namespace COVAR_Tecnologia.Hubs
{
    public class ChatHub : Hub
    {
        private readonly CoTecDBContext _context;

        public ChatHub(CoTecDBContext context)
        {
            _context = context;
        }

        public async Task JoinTicketGroup(string ticketId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, ticketId);
        }

        public async Task LeaveTicketGroup(string ticketId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, ticketId);
        }

        public async Task SendMessage(int ticketId, string textoMensaje, bool isAdmin)
        {
            if (string.IsNullOrWhiteSpace(textoMensaje)) return;

            var ticket = await _context.TicketsSoporte.FindAsync(ticketId);
            if (ticket == null || ticket.Estado != EstadoTicket.Abierto) return;

            var nuevoMensaje = new Mensaje
            {
                TicketSoporteId = ticketId,
                Texto = textoMensaje,
                FechaEnvio = DateTime.Now,
                EsRespuestaAdmin = isAdmin
            };

            _context.Add(nuevoMensaje);
            await _context.SaveChangesAsync();

            // Formatear la fecha para la UI
            string fechaFormateada = nuevoMensaje.FechaEnvio.ToString("dd/MM HH:mm");

            // Notificar a todos en la sala del ticket
            await Clients.Group(ticketId.ToString()).SendAsync("ReceiveMessage", textoMensaje, fechaFormateada, isAdmin);
        }
    }
}

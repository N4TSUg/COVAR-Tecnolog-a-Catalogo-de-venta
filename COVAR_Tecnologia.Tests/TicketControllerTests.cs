using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using COVAR_Tecnologia.Controllers;
using COVAR_Tecnologia.Data;
using COVAR_Tecnologia.Models;
using Xunit;

namespace COVAR_Tecnologia.Tests
{
    public class TicketControllerTests
    {
        private DbContextOptions<CoTecDBContext> GetDbContextOptions(string dbName)
        {
            return new DbContextOptionsBuilder<CoTecDBContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
        }

        [Fact]
        public async Task Index_DebeRetornarVista_ConListaDeTickets()
        {
            var options = GetDbContextOptions("TestDB_Ticket_Index");

            using (var context = new CoTecDBContext(options))
            {
                context.TicketsSoporte.Add(new cotec_ticketSoporte { Id = 1, Asunto = "Error" });
                await context.SaveChangesAsync();
            }

            using (var context = new CoTecDBContext(options))
            {
                var controller = new TicketController(context);
                var result = await controller.Index();

                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsAssignableFrom<IEnumerable<cotec_ticketSoporte>>(viewResult.ViewData.Model);
                Assert.Single(model);
            }
        }

        [Fact]
        public async Task ResponderGet_DebeRetornarNotFound_SiIdEsNulo()
        {
            var options = GetDbContextOptions("TestDB_Ticket_ResponderGet_NoId");
            using (var context = new CoTecDBContext(options))
            {
                var controller = new TicketController(context);
                var result = await controller.Responder(null);
                Assert.IsType<NotFoundResult>(result);
            }
        }

        [Fact]
        public async Task ResponderPost_DebeAgregarMensaje_Y_CerrarTicket()
        {
            var options = GetDbContextOptions("TestDB_Ticket_ResponderPost");

            using (var context = new CoTecDBContext(options))
            {
                context.TicketsSoporte.Add(new cotec_ticketSoporte { Id = 1, Asunto = "Duda", Estado = cotec_estadoTicket.Abierto });
                await context.SaveChangesAsync();
            }

            using (var context = new CoTecDBContext(options))
            {
                var controller = new TicketController(context);
                var result = await controller.Responder(1, "Respuesta desde test", true);

                var redirectResult = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal("Responder", redirectResult.ActionName);
            }

            using (var context = new CoTecDBContext(options))
            {
                var ticket = await context.TicketsSoporte.Include(t => t.Mensajes).FirstAsync();
                Assert.Equal(cotec_estadoTicket.Cerrado, ticket.Estado);
                Assert.Single(ticket.Mensajes);
                Assert.Equal("Respuesta desde test", ticket.Mensajes[0].Texto);
            }
        }
    }
}

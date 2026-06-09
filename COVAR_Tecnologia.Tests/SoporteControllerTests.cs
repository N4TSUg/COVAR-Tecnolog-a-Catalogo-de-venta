using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using COVAR_Tecnologia.Controllers;
using COVAR_Tecnologia.Data;
using COVAR_Tecnologia.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using Xunit;

namespace COVAR_Tecnologia.Tests
{
    public class SoporteControllerTests
    {
        private DbContextOptions<CoTecDBContext> GetDbContextOptions(string dbName)
        {
            return new DbContextOptionsBuilder<CoTecDBContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
        }

        private SoporteController CrearControladorConUsuario(CoTecDBContext context, string userId)
        {
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId),
            }, "mock"));

            var controller = new SoporteController(context)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext { User = user }
                }
            };
            return controller;
        }

        [Fact]
        public async Task Index_DebeRetornarTicketsDelUsuario()
        {
            var options = GetDbContextOptions("TestDB_Soporte_Index");

            using (var context = new CoTecDBContext(options))
            {
                context.TicketsSoporte.Add(new TicketSoporte { Id = 1, UsuarioId = 1, Asunto = "T1" });
                context.TicketsSoporte.Add(new TicketSoporte { Id = 2, UsuarioId = 2, Asunto = "T2" });
                await context.SaveChangesAsync();
            }

            using (var context = new CoTecDBContext(options))
            {
                var controller = CrearControladorConUsuario(context, "1");
                var result = await controller.Index();

                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsAssignableFrom<IEnumerable<TicketSoporte>>(viewResult.ViewData.Model);
                Assert.Single(model);
                Assert.Equal(1, model.First().Id);
            }
        }

        [Fact]
        public async Task CrearPost_DebeCrearTicketConMensajeInicial()
        {
            var options = GetDbContextOptions("TestDB_Soporte_CrearPost");

            using (var context = new CoTecDBContext(options))
            {
                var controller = CrearControladorConUsuario(context, "1");
                var result = await controller.Crear("Asunto test", "Mensaje inicial test");

                var redirectResult = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal("Index", redirectResult.ActionName);
            }

            using (var context = new CoTecDBContext(options))
            {
                var ticket = context.TicketsSoporte.Include(t => t.Mensajes).FirstOrDefault();
                Assert.NotNull(ticket);
                Assert.Equal("Asunto test", ticket.Asunto);
                Assert.Single(ticket.Mensajes);
                Assert.Equal("Mensaje inicial test", ticket.Mensajes[0].Texto);
            }
        }
    }
}

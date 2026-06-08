using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using COVAR_Tecnologia.Controllers;
using COVAR_Tecnologia.Data;
using COVAR_Tecnologia.Models;
using Xunit;

namespace COVAR_Tecnologia.Tests
{
    public class AdminControllerTests
    {
        private DbContextOptions<CoTecDBContext> GetDbContextOptions(string dbName)
        {
            return new DbContextOptionsBuilder<CoTecDBContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
        }

        [Fact]
        public async Task Index_DebeRetornarVista_ConTotalesCorrectos()
        {
            // Arrange
            var options = GetDbContextOptions("TestDB_Admin_Index");

            using (var context = new CoTecDBContext(options))
            {
                context.Productos.Add(new cotec_producto { Id = 1, Nombre = "Prod 1", Descripcion = "Desc", ImagenURL = "url" });
                context.Marcas.Add(new cotec_marca { Id = 1, Nombre = "Marca 1" });
                context.Categorias.Add(new cotec_categoria { Id = 1, Nombre = "Cat 1" });
                context.TicketsSoporte.Add(new cotec_ticketSoporte { Id = 1, Estado = cotec_estadoTicket.Abierto, Asunto = "Asunto" });
                context.TicketsSoporte.Add(new cotec_ticketSoporte { Id = 2, Estado = cotec_estadoTicket.Cerrado, Asunto = "Asunto" });
                context.Roles.Add(new cotec_rol { Id = 1, Nombre = "Rol" });
                context.Usuarios.Add(new cotec_usuario { Id = 1, Email = "test@test.com", Password = "pass", RolId = 1 });
                await context.SaveChangesAsync();
            }

            using (var context = new CoTecDBContext(options))
            {
                var controller = new AdminController(context);

                // Act
                var result = await controller.Index();

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                Assert.Equal(1, controller.ViewBag.TotalProductos);
                Assert.Equal(1, controller.ViewBag.TotalMarcas);
                Assert.Equal(1, controller.ViewBag.TotalCategorias);
                Assert.Equal(1, controller.ViewBag.TicketsAbiertos);
                Assert.Equal(1, controller.ViewBag.TotalUsuarios);
            }
        }
    }
}

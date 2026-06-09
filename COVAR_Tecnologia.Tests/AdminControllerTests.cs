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
                context.Productos.Add(new Producto { Id = 1, Nombre = "Prod 1", Descripcion = "Desc", ImagenURL = "url" });
                context.Marcas.Add(new Marca { Id = 1, Nombre = "Marca 1" });
                context.Categorias.Add(new Categoria { Id = 1, Nombre = "Cat 1" });
                context.TicketsSoporte.Add(new TicketSoporte { Id = 1, Estado = EstadoTicket.Abierto, Asunto = "Asunto" });
                context.TicketsSoporte.Add(new TicketSoporte { Id = 2, Estado = EstadoTicket.Cerrado, Asunto = "Asunto" });
                context.Roles.Add(new Rol { Id = 1, Nombre = "Rol" });
                context.Usuarios.Add(new Usuario { Id = 1, Email = "test@test.com", Password = "pass", RolId = 1 });
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

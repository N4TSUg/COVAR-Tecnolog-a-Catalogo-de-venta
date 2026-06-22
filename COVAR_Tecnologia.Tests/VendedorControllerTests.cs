using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using COVAR_Tecnologia.Controllers;
using COVAR_Tecnologia.Data;
using COVAR_Tecnologia.Models;
using Xunit;

namespace COVAR_Tecnologia.Tests
{
    public class VendedorControllerTests
    {
        private DbContextOptions<CoTecDBContext> GetDbContextOptions(string dbName)
        {
            return new DbContextOptionsBuilder<CoTecDBContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
        }

        [Fact]
        public async Task Index_DebeRetornarVista_ConMetricasVendedor()
        {
            // Arrange
            var options = GetDbContextOptions("TestDB_Vendedor_Index");

            using (var context = new CoTecDBContext(options))
            {
                context.Productos.Add(new Producto { Id = 1, Nombre = "Prod 1", Descripcion = "Desc", Precio = 10, ImagenURL = "url", MarcaId = 1, CategoriaId = 1 });
                context.TicketsSoporte.Add(new TicketSoporte { Id = 1, Estado = EstadoTicket.Abierto, Asunto = "Asunto" });
                await context.SaveChangesAsync();
            }

            using (var context = new CoTecDBContext(options))
            {
                var controller = new VendedorController(context);

                // Act
                var result = await controller.Index();

                // Assert
                var viewResult = Assert.IsType<ViewResult>(result);
                Assert.Equal(1, controller.ViewBag.TotalProductos);
                Assert.Equal(1, controller.ViewBag.TicketsPendientes);
            }
        }
    }
}

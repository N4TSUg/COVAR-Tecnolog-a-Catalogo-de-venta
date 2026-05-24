using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using COVAR_Tecnologia.Controllers;
using COVAR_Tecnologia.Data;
using COVAR_Tecnologia.Models;
using Xunit;
using System.Linq;
using System.Threading.Tasks;

namespace COVAR_Tecnologia.Tests
{
    public class HomeControllerTests
    {
        private DbContextOptions<CoTecDBContext> GetDbContextOptions(string dbName)
        {
            return new DbContextOptionsBuilder<CoTecDBContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
        }

        [Fact]
        public async Task Index_DebeRetornarVista_ConListaDeProductos()
        {
            var options = GetDbContextOptions("TestDB_Home_Index");

            using (var context = new CoTecDBContext(options))
            {
                context.Marcas.Add(new cotec_marca { Id = 1, Nombre = "Marca" });
                context.Categorias.Add(new cotec_categoria { Id = 1, Nombre = "Categoria" });
                context.Productos.Add(new cotec_producto { Id = 1, Nombre = "Producto 1", Descripcion = "Desc", ImagenURL = "url", MarcaId = 1, CategoriaId = 1 });
                context.Productos.Add(new cotec_producto { Id = 2, Nombre = "Producto 2", Descripcion = "Desc", ImagenURL = "url", MarcaId = 1, CategoriaId = 1 });
                await context.SaveChangesAsync();
            }

            using (var context = new CoTecDBContext(options))
            {
                var controller = new HomeController(context);
                var result = await controller.Index(null, null, null, 1);

                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsAssignableFrom<CatalogoViewModel>(viewResult.ViewData.Model);
                Assert.Equal(2, model.Productos.Count());
            }
        }

        [Fact]
        public async Task Detalle_SinId_DebeRetornarNotFound()
        {
            var options = GetDbContextOptions("TestDB_Home_Detalle_NoId");
            using (var context = new CoTecDBContext(options))
            {
                var controller = new HomeController(context);
                var result = await controller.Detalle(null);
                Assert.IsType<NotFoundResult>(result);
            }
        }

        [Fact]
        public async Task Detalle_ConIdExistente_DebeRetornarVistaConProducto()
        {
            var options = GetDbContextOptions("TestDB_Home_Detalle_Exito");
            using (var context = new CoTecDBContext(options))
            {
                context.Marcas.Add(new cotec_marca { Id = 1, Nombre = "Marca" });
                context.Categorias.Add(new cotec_categoria { Id = 1, Nombre = "Categoria" });
                context.Productos.Add(new cotec_producto { Id = 1, Nombre = "Producto 1", Descripcion = "Desc", ImagenURL = "url", MarcaId = 1, CategoriaId = 1 });
                await context.SaveChangesAsync();
            }

            using (var context = new CoTecDBContext(options))
            {
                var controller = new HomeController(context);
                var result = await controller.Detalle(1);

                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsAssignableFrom<cotec_producto>(viewResult.ViewData.Model);
                Assert.Equal(1, model.Id);
            }
        }

        [Fact]
        public void Privacy_DebeRetornarVista()
        {
            var options = GetDbContextOptions("TestDB_Home_Privacy");
            using (var context = new CoTecDBContext(options))
            {
                var controller = new HomeController(context);
                var result = controller.Privacy();
                Assert.IsType<ViewResult>(result);
            }
        }
    }
}

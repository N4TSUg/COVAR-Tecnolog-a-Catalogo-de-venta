using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using COVAR_Tecnologia.Controllers;
using COVAR_Tecnologia.Data;
using COVAR_Tecnologia.Models;
using Xunit;

namespace COVAR_Tecnologia.Tests
{
    public class ProductoControllerTests
    {
        private DbContextOptions<CoTecDBContext> GetDbContextOptions(string dbName)
        {
            return new DbContextOptionsBuilder<CoTecDBContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
        }

        // ==========================================
        // ❌ PRUEBAS DE FALLO (Unhappy Paths)
        // ==========================================

        [Fact]
        public async Task Crear_POST_ModeloInvalido_NoGuarda_Y_RecargaListasEnViewBag()
        {
            // 1. ARRANGE
            var options = GetDbContextOptions("TestDB_Prod_CrearFallo");
            using (var setupContext = new CoTecDBContext(options))
            {
                // Agregamos una marca y categoría de prueba para que los ViewBag tengan algo que cargar
                setupContext.Marcas.Add(new cotec_marca { Id = 1, Nombre = "Logitech" });
                setupContext.Categorias.Add(new cotec_categoria { Id = 1, Nombre = "Periférico" });
                await setupContext.SaveChangesAsync();
            }

            using var context = new CoTecDBContext(options);
            var controller = new ProductoController(context);

            // Forzamos un error de validación
            controller.ModelState.AddModelError("Precio", "El precio debe ser mayor a 0");
            var productoInvalido = new cotec_producto { Nombre = "Mouse", Precio = 0, MarcaId = 1, CategoriaId = 1 };

            // 2. ACT
            // Pasamos 'null' como archivo de imagen
            var result = await controller.Crear(productoInvalido, null);

            // 3. ASSERT
            var viewResult = Assert.IsType<ViewResult>(result);

            // Verificamos que el producto NO se guardó
            Assert.Empty(await context.Productos.ToListAsync());

            // ¡MUY IMPORTANTE! Verificamos que el controlador recargó las listas desplegables
            Assert.NotNull(viewResult.ViewData["Marcas"]);
            Assert.NotNull(viewResult.ViewData["Categorias"]);

            // Verificamos que sean del tipo SelectList que la vista espera
            Assert.IsType<SelectList>(viewResult.ViewData["Marcas"]);
        }

        [Fact]
        public async Task Editar_GET_IdNulo_RetornaNotFound()
        {
            var options = GetDbContextOptions("TestDB_Prod_EditarNulo");
            using var context = new CoTecDBContext(options);
            var controller = new ProductoController(context);

            var result = await controller.Editar((int?)null);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Editar_POST_IdDiferenteAlModelo_RetornaNotFound()
        {
            var options = GetDbContextOptions("TestDB_Prod_EditarHack");
            using var context = new CoTecDBContext(options);
            var controller = new ProductoController(context);

            var productoHackeado = new cotec_producto { Id = 5, Nombre = "Laptop" };

            var result = await controller.Editar(productoHackeado.Id);

            Assert.IsType<NotFoundResult>(result);
        }

        // ==========================================
        // ✅ PRUEBAS DE ÉXITO (Happy Paths)
        // ==========================================

        [Fact]
        public async Task Crear_POST_ModeloValido_SinArchivo_GuardaEnBD_Y_Redirige()
        {
            // 1. ARRANGE
            var options = GetDbContextOptions("TestDB_Prod_CrearExito");
            using var context = new CoTecDBContext(options);
            var controller = new ProductoController(context);

            var nuevoProducto = new cotec_producto
            {
                Id = 0,
                Nombre = "Teclado Mecánico",
                Descripcion = "Un excelente teclado RGB con switches azules.", // <-- CAMBIO AQUÍ: Agregado para cumplir la regla
                Precio = 150.50m,
                MarcaId = 1,
                CategoriaId = 1,
                ImagenURL = "https://url-de-internet.com/foto.jpg"
            };

            // 2. ACT
            var result = await controller.Crear(nuevoProducto, null);

            // 3. ASSERT
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);

            var productoGuardado = await context.Productos.FirstOrDefaultAsync(p => p.Nombre == "Teclado Mecánico");
            Assert.NotNull(productoGuardado);
            Assert.Equal("https://url-de-internet.com/foto.jpg", productoGuardado.ImagenURL);
        }

        [Fact]
        public async Task Detalle_GET_IdValido_RetornaVista_ConNavegacionIncluida()
        {
            // 1. ARRANGE
            var options = GetDbContextOptions("TestDB_Prod_DetalleExito");
            using (var setupContext = new CoTecDBContext(options))
            {
                var marca = new cotec_marca { Id = 1, Nombre = "Razer" };
                var cat = new cotec_categoria { Id = 1, Nombre = "Audio" };

                setupContext.Productos.Add(new cotec_producto
                {
                    Id = 1,
                    Nombre = "Audífonos Kraken",
                    Descripcion = "Audífonos gamer con sonido envolvente 7.1", // <-- CAMBIO AQUÍ
                    ImagenURL = "/images/kraken.jpg",                         // <-- CAMBIO AQUÍ
                    Precio = 200.00m,                                         // <-- CAMBIO AQUÍ (por buena práctica)
                    Marca = marca,
                    Categoria = cat
                });
                await setupContext.SaveChangesAsync();
            }

            using var context = new CoTecDBContext(options);
            var controller = new HomeController(context);

            // 2. ACT
            var result = await controller.Detalle(1);

            // 3. ASSERT
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<cotec_producto>(viewResult.Model);

            Assert.Equal("Audífonos Kraken", model.Nombre);
            Assert.NotNull(model.Marca);
            Assert.Equal("Razer", model.Marca.Nombre);
            Assert.NotNull(model.Categoria);
        }

        [Fact]
        public async Task Eliminar_GET_IdValido_RetornaVista()
        {
            var options = GetDbContextOptions("TestDB_Prod_EliminarGetExito");

            using (var setupContext = new CoTecDBContext(options))
            {
                setupContext.Marcas.Add(new cotec_marca { Id = 1, Nombre = "Marca" });
                setupContext.Categorias.Add(new cotec_categoria { Id = 1, Nombre = "Categoria" });
                setupContext.Productos.Add(new cotec_producto { Id = 1, Nombre = "ProductoTest", Descripcion = "Desc", ImagenURL = "url", MarcaId = 1, CategoriaId = 1 });
                await setupContext.SaveChangesAsync();
            }

            using var context = new CoTecDBContext(options);
            var controller = new ProductoController(context);

            var result = await controller.Eliminar(1);

            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<cotec_producto>(viewResult.Model);
            Assert.Equal(1, model.Id);
        }

        [Fact]
        public async Task EliminarConfirmado_POST_EliminaRegistro_Y_RedirigeAIndex()
        {
            var options = GetDbContextOptions("TestDB_Prod_EliminarPostExito");

            using (var setupContext = new CoTecDBContext(options))
            {
                setupContext.Marcas.Add(new cotec_marca { Id = 1, Nombre = "Marca" });
                setupContext.Categorias.Add(new cotec_categoria { Id = 1, Nombre = "Categoria" });
                setupContext.Productos.Add(new cotec_producto { Id = 1, Nombre = "ProductoTest", Descripcion = "Desc", ImagenURL = "url", MarcaId = 1, CategoriaId = 1 });
                await setupContext.SaveChangesAsync();
            }

            using var context = new CoTecDBContext(options);
            var controller = new ProductoController(context);

            var result = await controller.EliminarConfirmado(1);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);

            var productoEnBd = await context.Productos.FindAsync(1);
            Assert.Null(productoEnBd);
        }
    }
}
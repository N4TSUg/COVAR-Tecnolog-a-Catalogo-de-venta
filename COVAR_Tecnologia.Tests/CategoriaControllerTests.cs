using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using COVAR_Tecnologia.Controllers;
using COVAR_Tecnologia.Data;
using COVAR_Tecnologia.Models;
using Xunit;

namespace COVAR_Tecnologia.Tests
{
    public class CategoriaControllerTests
    {
        // MÃ©todo auxiliar para crear bases de datos aisladas
        private DbContextOptions<CoTecDBContext> GetDbContextOptions(string dbName)
        {
            return new DbContextOptionsBuilder<CoTecDBContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
        }

        // ==========================================
        // âŒ PRUEBAS DE FALLO (Unhappy Paths)
        // ==========================================

        [Fact]
        public async Task Crear_POST_ModeloInvalido_NoGuardaEnBD_Y_RetornaVista()
        {
            // 1. ARRANGE
            var options = GetDbContextOptions("TestDB_Cat_CrearFallo");
            using var context = new CoTecDBContext(options);
            var controller = new CategoriaController(context);

            // Simulamos que el usuario dejÃ³ el nombre en blanco y ASP.NET detecta el error
            controller.ModelState.AddModelError("Nombre", "El nombre de la categorÃ­a es obligatorio");
            var categoriaInvalida = new Categoria { Id = 0, Nombre = "" };

            // 2. ACT
            var result = await controller.Crear(categoriaInvalida);

            // 3. ASSERT
            var viewResult = Assert.IsType<ViewResult>(result); // Retorna la vista, no redirige
            Assert.Equal(categoriaInvalida, viewResult.Model); // Devuelve los mismos datos ingresados
            Assert.Empty(context.Categorias); // Verificamos que la BD siga vacÃ­a
        }

        [Fact]
        public async Task Editar_GET_IdNulo_RetornaNotFound()
        {
            // 1. ARRANGE
            var options = GetDbContextOptions("TestDB_Cat_EditarNulo");
            using var context = new CoTecDBContext(options);
            var controller = new CategoriaController(context);

            // 2. ACT
            var result = await controller.Editar((int?)null);

            // 3. ASSERT
            Assert.IsType<NotFoundResult>(result); // Bloquea el acceso si no hay ID
        }

        [Fact]
        public async Task Editar_POST_IdDiferenteAlModelo_RetornaNotFound()
        {
            // 1. ARRANGE
            var options = GetDbContextOptions("TestDB_Cat_EditarHack");
            using var context = new CoTecDBContext(options);
            var controller = new CategoriaController(context);

            // Simulamos un formulario manipulado con ID 99
            var categoriaHackeada = new Categoria { Id = 99, Nombre = "Hack" };

            // 2. ACT
            // Simulamos que la URL original apuntaba al ID 1
            var result = await controller.Editar(1, categoriaHackeada);

            // 3. ASSERT
            Assert.IsType<NotFoundResult>(result); // Detecta la manipulaciÃ³n y lanza NotFound
        }

        // ==========================================
        // âœ… PRUEBAS DE Ã‰XITO (Happy Paths)
        // ==========================================

        [Fact]
        public async Task Crear_POST_ModeloValido_GuardaEnBD_Y_RedirigeAIndex()
        {
            // 1. ARRANGE
            var options = GetDbContextOptions("TestDB_Cat_CrearExito");
            using var context = new CoTecDBContext(options);
            var controller = new CategoriaController(context);

            var nuevaCategoria = new Categoria { Id = 0, Nombre = "Almacenamiento" };

            // 2. ACT
            var result = await controller.Crear(nuevaCategoria);

            // 3. ASSERT
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName); // Redirige a la tabla

            var categoriaGuardada = await context.Categorias.FirstOrDefaultAsync(c => c.Nombre == "Almacenamiento");
            Assert.NotNull(categoriaGuardada); // Comprobamos que sÃ­ se guardÃ³ fÃ­sicamente
        }

        [Fact]
        public async Task Editar_GET_IdValido_RetornaVista_ConLaCategoriaCorrecta()
        {
            // 1. ARRANGE
            var options = GetDbContextOptions("TestDB_Cat_EditarGetExito");

            using (var setupContext = new CoTecDBContext(options))
            {
                setupContext.Categorias.Add(new Categoria { Id = 1, Nombre = "Monitores" });
                await setupContext.SaveChangesAsync();
            }

            using var context = new CoTecDBContext(options);
            var controller = new CategoriaController(context);

            // 2. ACT
            var result = await controller.Editar(1);

            // 3. ASSERT
            var viewResult = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<Categoria>(viewResult.Model);
            Assert.Equal("Monitores", model.Nombre); // Validamos que el formulario se llene con "Monitores"
            Assert.Equal(1, model.Id);
        }

        [Fact]
        public async Task Editar_POST_ModeloValido_ActualizaBD_Y_RedirigeAIndex()
        {
            // 1. ARRANGE
            var options = GetDbContextOptions("TestDB_Cat_EditarPostExito");

            using (var setupContext = new CoTecDBContext(options))
            {
                setupContext.Categorias.Add(new Categoria { Id = 1, Nombre = "Laptops" });
                await setupContext.SaveChangesAsync();
            }

            using var context = new CoTecDBContext(options);
            var controller = new CategoriaController(context);

            var categoriaEditada = new Categoria { Id = 1, Nombre = "Laptops Gamers" };

            // 2. ACT
            var result = await controller.Editar(1, categoriaEditada);

            // 3. ASSERT
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);

            var categoriaEnBd = await context.Categorias.FindAsync(1);
            Assert.Equal("Laptops Gamers", categoriaEnBd.Nombre); // Validamos que la actualizaciÃ³n fue exitosa
        }[Fact]
        public async Task EliminarConfirmado_POST_EliminaRegistro_Y_RedirigeAIndex()
        {
            var options = GetDbContextOptions("TestDB_Cat_EliminarPostExito");

            using (var setupContext = new CoTecDBContext(options))
            {
                setupContext.Categorias.Add(new Categoria { Id = 1, Nombre = "Ratones" });
                await setupContext.SaveChangesAsync();
            }

            using var context = new CoTecDBContext(options);
            var controller = new CategoriaController(context);

            var result = await controller.EliminarConfirmado(1);

            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);

            var categoriaEnBd = await context.Categorias.FindAsync(1);
            Assert.Null(categoriaEnBd);
        }
    }
}

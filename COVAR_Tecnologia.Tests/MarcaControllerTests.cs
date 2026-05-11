using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using COVAR_Tecnologia.Controllers;
using COVAR_Tecnologia.Data;
using COVAR_Tecnologia.Models;

namespace COVAR_Tecnologia.Tests
{
    public class MarcaControllerTests
    {
        // Método auxiliar para configurar la base de datos en memoria
        private DbContextOptions<CoTecDBContext> GetDbContextOptions(string dbName)
        {
            return new DbContextOptionsBuilder<CoTecDBContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
        }

        [Fact]
        public async Task Index_DebeRetornarVista_ConListaDeMarcas()
        {
            // 1. ARRANGE (Preparar el entorno)
            var options = GetDbContextOptions("TestDB_Marcas_Index");

            // Insertamos datos falsos en la BD en memoria
            using (var context = new CoTecDBContext(options))
            {
                context.Marcas.Add(new cotec_marca { Id = 1, Nombre = "Logitech" });
                context.Marcas.Add(new cotec_marca { Id = 2, Nombre = "Razer" });
                await context.SaveChangesAsync();
            }

            // 2. ACT (Ejecutar el método que queremos probar)
            using (var context = new CoTecDBContext(options))
            {
                var controller = new MarcaController(context);

                // Llamamos al método Index del controlador
                var result = await controller.Index();

                // 3. ASSERT (Afirmar / Verificar resultados)
                // Verificamos que el resultado sea una Vista (ViewResult)
                var viewResult = Assert.IsType<ViewResult>(result);

                // Verificamos que el modelo enviado a la vista sea una lista de marcas
                var model = Assert.IsAssignableFrom<IEnumerable<cotec_marca>>(viewResult.ViewData.Model);

                // Verificamos que la lista contenga exactamente las 2 marcas que insertamos
                Assert.Equal(2, model.Count());
            }
        }

        [Fact]
        public async Task Crear_POST_ModeloValido_GuardaEnBD_Y_RedirigeAIndex()
        {
            // 1. ARRANGE
            var options = GetDbContextOptions("TestDB_Marcas_CrearExito");
            using var context = new CoTecDBContext(options);
            var controller = new MarcaController(context);

            // Simulamos una marca nueva (con Id 0 porque la base de datos se encarga de asignarlo)
            var nuevaMarca = new cotec_marca { Id = 0, Nombre = "Corsair" };

            // 2. ACT
            var result = await controller.Crear(nuevaMarca);

            // 3. ASSERT
            // Comprobamos que el resultado sea una redirección (RedirectToAction)
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);

            // Comprobamos que la marca realmente se haya guardado en nuestra base de datos en memoria
            var marcaGuardada = await context.Marcas.FirstOrDefaultAsync(m => m.Nombre == "Corsair");
            Assert.NotNull(marcaGuardada);
        }
    
        [Fact]
        public async Task Crear_POST_ModeloInvalido_NoGuardaEnBD_Y_RetornaVista()
        {
            // 1. ARRANGE
            var options = GetDbContextOptions("TestDB_Marcas_CrearFallo");
            using var context = new CoTecDBContext(options);
            var controller = new MarcaController(context);

            // Forzamos manualmente un error en el ModelState, simulando que la validación falló
            controller.ModelState.AddModelError("Nombre", "El nombre de la marca es obligatorio");

            var marcaInvalida = new cotec_marca { Id = 0, Nombre = "" };

            // 2. ACT
            var result = await controller.Crear(marcaInvalida);

            // 3. ASSERT
            // Verificamos que retorne un ViewResult (y NO un RedirectToAction)
            var viewResult = Assert.IsType<ViewResult>(result);

            // Verificamos que la vista reciba el mismo modelo con el que intentamos fallar
            Assert.Equal(marcaInvalida, viewResult.Model);

            // Lo más importante: Verificamos que la marca NO se haya guardado en la base de datos
            Assert.Empty(context.Marcas);
        }

        [Fact]
        public async Task Editar_GET_IdValido_RetornaVista_ConLaMarcaCorrecta()
        {
            // 1. ARRANGE
            var options = GetDbContextOptions("TestDB_Marcas_EditarGetExito");

            // Preparamos la base de datos insertando un registro primero
            using (var setupContext = new CoTecDBContext(options))
            {
                setupContext.Marcas.Add(new cotec_marca { Id = 1, Nombre = "Asus" });
                await setupContext.SaveChangesAsync();
            }

            using var context = new CoTecDBContext(options);
            var controller = new MarcaController(context);

            // 2. ACT
            // Simulamos que el usuario entra a la URL /Marca/Editar/1
            var result = await controller.Editar(1);

            // 3. ASSERT
            var viewResult = Assert.IsType<ViewResult>(result);

            // Verificamos que el modelo que se envía a la vista sea de tipo cotec_marca
            var model = Assert.IsType<cotec_marca>(viewResult.Model);

            // Confirmamos que trajo la marca correcta
            Assert.Equal("Asus", model.Nombre);
            Assert.Equal(1, model.Id);
        }

        [Fact]
        public async Task Editar_GET_IdNulo_RetornaNotFound()
        {
            // 1. ARRANGE
            var options = GetDbContextOptions("TestDB_Marcas_EditarNulo");
            using var context = new CoTecDBContext(options);
            var controller = new MarcaController(context);

            // 2. ACT
            // Le pasamos un nulo explícito simulando que faltó en la URL
            var result = await controller.Editar((int?)null);

            // 3. ASSERT
            // Verificamos que el controlador detenga la ejecución y devuelva un NotFound
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Editar_POST_ModeloValido_ActualizaBD_Y_RedirigeAIndex()
        {
            // 1. ARRANGE
            var options = GetDbContextOptions("TestDB_Marcas_EditarPostExito");

            // Insertamos la marca original
            using (var setupContext = new CoTecDBContext(options))
            {
                setupContext.Marcas.Add(new cotec_marca { Id = 1, Nombre = "Marca Vieja" });
                await setupContext.SaveChangesAsync();
            }

            using var context = new CoTecDBContext(options);
            var controller = new MarcaController(context);

            // Creamos el objeto con los datos ya modificados por el usuario
            var marcaEditada = new cotec_marca { Id = 1, Nombre = "Marca Renombrada" };

            // 2. ACT
            var result = await controller.Editar(1, marcaEditada);

            // 3. ASSERT
            // Comprobamos la redirección
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);

            // Verificamos que en la base de datos el nombre haya cambiado realmente
            var marcaEnBd = await context.Marcas.FindAsync(1);
            Assert.Equal("Marca Renombrada", marcaEnBd.Nombre);
        }

        [Fact]
        public async Task Editar_POST_IdDiferenteAlModelo_RetornaNotFound()
        {
            // 1. ARRANGE
            var options = GetDbContextOptions("TestDB_Marcas_EditarHack");
            using var context = new CoTecDBContext(options);
            var controller = new MarcaController(context);

            // Simulamos los datos que vienen del formulario con ID = 5
            var marcaModificada = new cotec_marca { Id = 5, Nombre = "Marca Hackeada" };

            // 2. ACT
            // Simulamos que la URL dice "/Marca/Editar/1", pero el modelo trae el ID 5
            var result = await controller.Editar(1, marcaModificada);

            // 3. ASSERT
            // Verificamos que el sistema detecte la discrepancia y devuelva un NotFound
            Assert.IsType<NotFoundResult>(result);
        }
        // ==========================================
        // ❌ PRUEBA DE FALLO: ELIMINAR UN PASO
        // ==========================================

        [Fact]
        public async Task Eliminar_IdNoExiste_NoHaceNada_Y_RedirigeAIndex()
        {
            // 1. ARRANGE
            var options = GetDbContextOptions("TestDB_Marcas_EliminarNoExisteUnPaso");
            using var context = new CoTecDBContext(options);
            var controller = new MarcaController(context);

            // 2. ACT
            // Intentamos borrar un ID que no existe
            var result = await controller.Eliminar(999);

            // 3. ASSERT
            // Como tu controlador no usa NotFound, verificamos que simplemente nos redirija de vuelta
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);
        }

        // ==========================================
        // ✅ PRUEBA DE ÉXITO: ELIMINAR UN PASO
        // ==========================================

        [Fact]
        public async Task Eliminar_IdValido_BorraDeBD_Y_RedirigeAIndex()
        {
            // 1. ARRANGE
            var options = GetDbContextOptions("TestDB_Marcas_EliminarExitoUnPaso");
            using (var setupContext = new CoTecDBContext(options))
            {
                setupContext.Marcas.Add(new cotec_marca { Id = 1, Nombre = "HyperX" });
                await setupContext.SaveChangesAsync();
            }

            using var context = new CoTecDBContext(options);
            var controller = new MarcaController(context);

            // 2. ACT
            // Borramos el ID 1
            var result = await controller.Eliminar(1);

            // 3. ASSERT
            // Verificamos la redirección
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirectToActionResult.ActionName);

            // Verificamos que la base de datos realmente haya borrado el registro
            var marcaEnBd = await context.Marcas.FindAsync(1);
            Assert.Null(marcaEnBd);
            Assert.Empty(context.Marcas);
        }
    }
}
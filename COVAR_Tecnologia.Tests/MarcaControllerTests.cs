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
    }
}
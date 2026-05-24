using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using COVAR_Tecnologia.Controllers;
using COVAR_Tecnologia.Data;
using COVAR_Tecnologia.Models;
using Xunit;

namespace COVAR_Tecnologia.Tests
{
    public class UsuarioControllerTests
    {
        private DbContextOptions<CoTecDBContext> GetDbContextOptions(string dbName)
        {
            return new DbContextOptionsBuilder<CoTecDBContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
        }

        [Fact]
        public async Task Index_DebeRetornarVista_ConListaDeUsuarios()
        {
            var options = GetDbContextOptions("TestDB_Usuario_Index");

            using (var context = new CoTecDBContext(options))
            {
                context.Roles.Add(new cotec_rol { Id = 1, Nombre = "Rol" });
                context.Usuarios.Add(new cotec_usuario { Id = 1, Email = "test1@test.com", Password = "pass", RolId = 1 });
                await context.SaveChangesAsync();
            }

            using (var context = new CoTecDBContext(options))
            {
                var controller = new UsuarioController(context);
                var result = await controller.Index();

                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsAssignableFrom<IEnumerable<cotec_usuario>>(viewResult.ViewData.Model);
                Assert.Single(model);
            }
        }

        [Fact]
        public void CrearGet_DebeRetornarVista_ConRolesEnViewBag()
        {
            var options = GetDbContextOptions("TestDB_Usuario_CrearGet");

            using (var context = new CoTecDBContext(options))
            {
                context.Roles.Add(new cotec_rol { Id = 1, Nombre = "Administrador" });
                context.Roles.Add(new cotec_rol { Id = 2, Nombre = "Vendedor" });
                context.SaveChanges();
            }

            using (var context = new CoTecDBContext(options))
            {
                var controller = new UsuarioController(context);
                var result = controller.Crear();

                var viewResult = Assert.IsType<ViewResult>(result);
                Assert.NotNull(viewResult.ViewData["Roles"]);
            }
        }

        [Fact]
        public async Task CrearPost_ModeloValido_DebeCrearYRedirigir()
        {
            var options = GetDbContextOptions("TestDB_Usuario_CrearPost");
            var nuevoUsuario = new cotec_usuario { Email = "nuevo@test.com", Password = "pass" };

            using (var context = new CoTecDBContext(options))
            {
                var controller = new UsuarioController(context);
                var result = await controller.Crear(nuevoUsuario, "clave123");

                var redirectResult = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal("Index", redirectResult.ActionName);
            }

            using (var context = new CoTecDBContext(options))
            {
                Assert.Equal(1, context.Usuarios.Count());
            }
        }

        [Fact]
        public async Task EliminarGet_ConIdValido_DebeRetornarVista()
        {
            var options = GetDbContextOptions("TestDB_Usuario_EliminarGet");

            using (var context = new CoTecDBContext(options))
            {
                context.Roles.Add(new cotec_rol { Id = 1, Nombre = "Rol" });
                context.Usuarios.Add(new cotec_usuario { Id = 1, Email = "test@test.com", Password = "pass", RolId = 1 });
                await context.SaveChangesAsync();
            }

            using (var context = new CoTecDBContext(options))
            {
                var controller = new UsuarioController(context);
                var result = await controller.Eliminar(1);

                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsAssignableFrom<cotec_usuario>(viewResult.ViewData.Model);
                Assert.Equal(1, model.Id);
            }
        }

        [Fact]
        public async Task EliminarConfirmado_DebeBorrarUsuario()
        {
            var options = GetDbContextOptions("TestDB_Usuario_EliminarConfirmado");

            using (var context = new CoTecDBContext(options))
            {
                context.Roles.Add(new cotec_rol { Id = 1, Nombre = "Rol" });
                context.Usuarios.Add(new cotec_usuario { Id = 1, Email = "test@test.com", Password = "pass", RolId = 1 });
                context.Usuarios.Add(new cotec_usuario { Id = 2, Email = "enriquearana1402@gmail.com", Password = "pass", RolId = 1 });
                await context.SaveChangesAsync();
            }

            using (var context = new CoTecDBContext(options))
            {
                var controller = new UsuarioController(context);
                var result = await controller.EliminarConfirmado(1);

                var redirectResult = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal("Index", redirectResult.ActionName);
            }

            using (var context = new CoTecDBContext(options))
            {
                Assert.Equal(1, context.Usuarios.Count()); // El usuario 2 quedó intacto
            }
        }
    }
}

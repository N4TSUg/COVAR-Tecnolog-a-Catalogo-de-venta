using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Rendering;
using COVAR_Tecnologia.Controllers;
using COVAR_Tecnologia.Data;
using COVAR_Tecnologia.Models;
using Xunit;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

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
        public async Task EditarGet_DebeRetornarVista_ConUsuarioValido()
        {
            var options = GetDbContextOptions("TestDB_Usuario_EditarGet");

            using (var context = new CoTecDBContext(options))
            {
                context.Roles.Add(new cotec_rol { Id = 1, Nombre = "Administrador" });
                context.Usuarios.Add(new cotec_usuario { Id = 1, Email = "test@test.com", Password = "pass", RolId = 1 });
                await context.SaveChangesAsync();
            }

            using (var context = new CoTecDBContext(options))
            {
                var controller = new UsuarioController(context);
                var result = await controller.Editar(1);

                var viewResult = Assert.IsType<ViewResult>(result);
                var model = Assert.IsType<cotec_usuario>(viewResult.Model);
                Assert.Equal(1, model.Id);
            }
        }

        [Fact]
        public async Task EditarPost_ModeloValido_DebeEditarYRedirigir()
        {
            var options = GetDbContextOptions("TestDB_Usuario_EditarPost");

            using (var context = new CoTecDBContext(options))
            {
                context.Roles.Add(new cotec_rol { Id = 1, Nombre = "Administrador" });
                context.Usuarios.Add(new cotec_usuario { Id = 1, Email = "viejo@test.com", Password = "pass", RolId = 1 });
                await context.SaveChangesAsync();
            }

            using (var context = new CoTecDBContext(options))
            {
                var controller = new UsuarioController(context);
                var usuarioEditado = new cotec_usuario { Id = 1, Email = "nuevo@test.com", Password = "pass", RolId = 1 };
                var result = await controller.Editar(1, usuarioEditado, "nuevaclave");

                var redirectResult = Assert.IsType<RedirectToActionResult>(result);
                Assert.Equal("Index", redirectResult.ActionName);
            }

            using (var context = new CoTecDBContext(options))
            {
                var usuario = context.Usuarios.First();
                Assert.Equal("nuevo@test.com", usuario.Email);
                // "nuevaclave" se encriptaría, así que el password será distinto de "pass"
                Assert.NotEqual("pass", usuario.Password);
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

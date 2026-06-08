using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using COVAR_Tecnologia.Controllers;
using COVAR_Tecnologia.Data;
using COVAR_Tecnologia.Models;
using Xunit;

namespace COVAR_Tecnologia.Tests
{
    public class AccesoControllerTests
    {
        private DbContextOptions<CoTecDBContext> GetDbContextOptions(string dbName)
        {
            return new DbContextOptionsBuilder<CoTecDBContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
        }

        // --- PRUEBAS DE REGISTRO (FALLOS) ---

        [Fact]
        public async Task Registrarse_POST_ClavesNoCoinciden_RetornaVistaConError()
        {
            // 1. ARRANGE
            var options = GetDbContextOptions("TestDB_Acceso_ClavesDistintas");
            using var context = new CoTecDBContext(options);
            var controller = new AccesoController(context);
            var nuevoUsuario = new cotec_usuario { Email = "test@covar.com" };

            // 2. ACT
            // Simulamos que el usuario escribió contraseñas diferentes
            var result = await controller.Registrarse(nuevoUsuario, "123456", "654321");

            // 3. ASSERT
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Las contraseñas no coinciden.", controller.ViewData["Mensaje"]);
            Assert.Empty(context.Usuarios); // Nadie se guardó
        }

        [Fact]
        public async Task Registrarse_POST_CorreoYaExiste_RetornaVistaConError()
        {
            // 1. ARRANGE
            var options = GetDbContextOptions("TestDB_Acceso_CorreoDuplicado");
            using (var setupContext = new CoTecDBContext(options))
            {
                // Inyectamos el rol y un usuario ya existente
                setupContext.Roles.Add(new cotec_rol { Id = 1, Nombre = "Cliente" });
                setupContext.Usuarios.Add(new cotec_usuario { Id = 1, Email = "existe@covar.com", Password = "hash" });
                await setupContext.SaveChangesAsync();
            }

            using var context = new CoTecDBContext(options);
            var controller = new AccesoController(context);
            var usuarioDuplicado = new cotec_usuario { Email = "existe@covar.com" };

            // 2. ACT
            var result = await controller.Registrarse(usuarioDuplicado, "123456", "123456");

            // 3. ASSERT
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Este correo ya está registrado.", controller.ViewData["Mensaje"]);

            // Verificamos que siga habiendo solo 1 usuario (el original), no se creó el duplicado
            Assert.Equal(1, await context.Usuarios.CountAsync());
        }

        // --- PRUEBAS DE LOGIN (FALLOS) ---

        [Fact]
        public async Task Login_POST_UsuarioNoExiste_RetornaVistaConError()
        {
            // 1. ARRANGE
            var options = GetDbContextOptions("TestDB_Acceso_LoginNoExiste");
            using var context = new CoTecDBContext(options);
            var controller = new AccesoController(context);

            // 2. ACT
            // Intentamos entrar con un correo que no está en la BD
            var result = await controller.Login("fantasma@covar.com", "123456");

            // 3. ASSERT
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Correo o contraseña incorrectos.", controller.ViewData["Mensaje"]);
        }

        [Fact]
        public async Task Login_POST_ClaveIncorrecta_RetornaVistaConError()
        {
            // 1. ARRANGE
            var options = GetDbContextOptions("TestDB_Acceso_LoginClaveMal");
            using (var setupContext = new CoTecDBContext(options))
            {
                setupContext.Usuarios.Add(new cotec_usuario
                {
                    Id = 1,
                    Email = "real@covar.com",
                    // Guardamos la contraseña YA hasheada, como estaría en la vida real
                    Password = BCrypt.Net.BCrypt.HashPassword("ClaveCorrecta123")
                });
                await setupContext.SaveChangesAsync();
            }

            using var context = new CoTecDBContext(options);
            var controller = new AccesoController(context);

            // 2. ACT
            // Intentamos entrar con el correo correcto pero mala clave
            var result = await controller.Login("real@covar.com", "ClaveEquivocada");

            // 3. ASSERT
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal("Correo o contraseña incorrectos.", controller.ViewData["Mensaje"]);
        }

        [Fact]
        public async Task Registrarse_POST_DatosValidos_GuardaUsuario_EncriptaClave_Y_Redirige()
        {
            // 1. ARRANGE
            var options = GetDbContextOptions("TestDB_Acceso_RegistroExito");
            using (var setupContext = new CoTecDBContext(options))
            {
                // El sistema necesita que el rol "Cliente" exista para poder asignarlo
                setupContext.Roles.Add(new cotec_rol { Id = 1, Nombre = "Cliente" });
                await setupContext.SaveChangesAsync();
            }

            using var context = new CoTecDBContext(options);
            var controller = new AccesoController(context);
            var nuevoUsuario = new cotec_usuario { Email = "nuevo@covar.com" };
            string clavePlana = "MiClaveSecreta";

            // 2. ACT
            var result = await controller.Registrarse(nuevoUsuario, clavePlana, clavePlana);

            // 3. ASSERT
            // Verificamos redirección al Login
            var redirectToActionResult = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Login", redirectToActionResult.ActionName);
            Assert.Equal("Acceso", redirectToActionResult.ControllerName);

            // Verificamos la base de datos
            var usuarioGuardado = await context.Usuarios.FirstOrDefaultAsync(u => u.Email == "nuevo@covar.com");

            Assert.NotNull(usuarioGuardado);
            Assert.Equal(1, usuarioGuardado.RolId); // Se le asignó el rol cliente correctamente

            // LA PRUEBA DE FUEGO DE SEGURIDAD:
            // 1. Asegurarnos que la contraseña NO se guardó en texto plano
            Assert.NotEqual(clavePlana, usuarioGuardado.Password);
            // 2. Asegurarnos que el Hash generado realmente corresponde a la clave que escribió
            Assert.True(BCrypt.Net.BCrypt.Verify(clavePlana, usuarioGuardado.Password));
        }
    }
}
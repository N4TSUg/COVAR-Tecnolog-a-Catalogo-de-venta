using COVAR_Tecnologia.Models;
using Microsoft.EntityFrameworkCore;

namespace COVAR_Tecnologia.Data
{
    public class CoTecDBContext : DbContext
    {
        public CoTecDBContext(DbContextOptions<CoTecDBContext>options) : base(options) { }

        // Estas propiedades DbSet representan cada tabla en tu base de datos SQL
        public DbSet<cotec_usuario> Usuarios { get; set; }
        public DbSet<cotec_rol> Roles { get; set; }
        public DbSet<cotec_producto> Productos { get; set; }
        public DbSet<cotec_marca> Marcas { get; set; }
        public DbSet<cotec_categoria> Categorias { get; set; }
        public DbSet<cotec_ticketSoporte> TicketsSoporte { get; set; }
        public DbSet<cotec_mensaje> MensajesSoporte { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Siempre es buena práctica llamar al método base primero
            base.OnModelCreating(modelBuilder);

            // 1. Relación TicketSoporte -> Mensajes
            // Si se borra un ticket, está bien que se borren todos sus mensajes (Cascade)
            modelBuilder.Entity<cotec_mensaje>()
                .HasOne(m => m.TicketSoporte)
                .WithMany(t => t.Mensajes)
                .HasForeignKey(m => m.TicketSoporteId)
                .OnDelete(DeleteBehavior.Cascade);

            // 2. Relación Usuario -> TicketsSoporte
            // Evitamos borrar los tickets accidentalmente si se elimina un usuario (Restrict)
            modelBuilder.Entity<cotec_ticketSoporte>()
                .HasOne(t => t.Usuario)
                .WithMany(u => u.Tickets)
                .HasForeignKey(t => t.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // 3. Relación Producto -> Marca
            // Evitamos borrar productos si se elimina una marca por error (Restrict)
            modelBuilder.Entity<cotec_producto>()
                .HasOne(p => p.Marca)
                .WithMany(m => m.Productos)
                .HasForeignKey(p => p.MarcaId)
                .OnDelete(DeleteBehavior.Restrict);

            // 4. Relación Producto -> Categoria
            // Evitamos borrar productos si se elimina una categoría (Restrict)
            modelBuilder.Entity<cotec_producto>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);
            // 5. Relación Rol -> Usuario
            modelBuilder.Entity<cotec_usuario>()
            .HasOne(u => u.Rol)
            .WithMany(r => r.Usuarios)
            .HasForeignKey(u => u.RolId)
            .OnDelete(DeleteBehavior.Restrict);
            //6. Creación de roles por defecto
            modelBuilder.Entity<cotec_rol>().HasData(
            new cotec_rol { Id = 1, Nombre = "Administrador" },
            new cotec_rol { Id = 2, Nombre = "Vendedor" },
            new cotec_rol { Id = 3, Nombre = "Cliente" });
            //7. Creacion de credenciales de administrador
            modelBuilder.Entity<cotec_usuario>().HasData(
                new cotec_usuario
                {
                    Id = 1,
                    Email = "enriquearana1402@gmail.com",
                    // IMPORTANTE: Aquí va el hash, no la contraseña real "3nriqueA1402"
                    Password = "$2a$12$tzU3g/s8DF2nU3Wu4t5sRuJ0j4jmhmrG0FZBhvadEgNe2Z0PUQnjq",
                    RolId = 1 // 1 corresponde a Administrador
                }
            );
        }
    }
}

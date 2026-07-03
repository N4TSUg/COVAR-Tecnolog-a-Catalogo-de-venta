using COVAR_Tecnologia.Models;
using Microsoft.EntityFrameworkCore;

namespace COVAR_Tecnologia.Data
{
    public class CoTecDBContext : DbContext
    {
        public CoTecDBContext(DbContextOptions<CoTecDBContext> options) : base(options) { }

        // Estas propiedades DbSet representan cada tabla en tu base de datos SQL
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Marca> Marcas { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<TicketSoporte> TicketsSoporte { get; set; }
        public DbSet<Mensaje> MensajesSoporte { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Siempre es buena práctica llamar al método base primero
            base.OnModelCreating(modelBuilder);

            // 1. Relación TicketSoporte -> Mensajes
            // Si se borra un ticket, está bien que se borren todos sus mensajes (Cascade)
            modelBuilder.Entity<Mensaje>()
                .HasOne(m => m.TicketSoporte)
                .WithMany(t => t.Mensajes)
                .HasForeignKey(m => m.TicketSoporteId)
                .OnDelete(DeleteBehavior.Cascade);

            // 2. Relación Usuario -> TicketsSoporte
            // Evitamos borrar los tickets accidentalmente si se elimina un usuario (Restrict)
            modelBuilder.Entity<TicketSoporte>()
                .HasOne(t => t.Usuario)
                .WithMany(u => u.Tickets)
                .HasForeignKey(t => t.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            // 2.5. Relación Producto -> TicketsSoporte
            modelBuilder.Entity<TicketSoporte>()
                .HasOne(t => t.Producto)
                .WithMany()
                .HasForeignKey(t => t.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);

            // 3. Relación Producto -> Marca
            // Evitamos borrar productos si se elimina una marca por error (Restrict)
            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Marca)
                .WithMany(m => m.Productos)
                .HasForeignKey(p => p.MarcaId)
                .OnDelete(DeleteBehavior.Restrict);

            // 4. Relación Producto -> Categoria
            // Evitamos borrar productos si se elimina una categoría (Restrict)
            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);
            // 5. Relación Rol -> Usuario
            modelBuilder.Entity<Usuario>()
            .HasOne(u => u.Rol)
            .WithMany(r => r.Usuarios)
            .HasForeignKey(u => u.RolId)
            .OnDelete(DeleteBehavior.Restrict);
            //6. Creación de roles por defecto
            modelBuilder.Entity<Rol>().HasData(
            new Rol { Id = 1, Nombre = "Administrador" },
            new Rol { Id = 2, Nombre = "Vendedor" },
            new Rol { Id = 3, Nombre = "Cliente" });
            //7. Creacion de credenciales de administrador
            modelBuilder.Entity<Usuario>().HasData(
                new Usuario
                {
                    Id = 1,
                    Email = "enriquearana1402@gmail.com",
                    // IMPORTANTE: Aquí va el hash, no la contraseña real "examplepass"
                    Password = "$2a$12$tzU3g/s8DF2nU3Wu4t5sRuJ0j4jmhmrG0FZBhvadEgNe2Z0PUQnjq", //Aquí va el hash de tu contraseña
                    RolId = 1 // 1 corresponde a Administrador
                }
            );
            //8.Relación de Producto -> TicketSoporte
            modelBuilder.Entity<TicketSoporte>()
            .HasOne(t => t.Producto)
            .WithMany() // O WithMany(p => p.Tickets) si decides añadir una lista de tickets en el modelo Producto
            .HasForeignKey(t => t.ProductoId)
            .OnDelete(DeleteBehavior.Restrict);
        }
    }
}

using Inventory.Api.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Api.Data
{
    public class DataContext : IdentityDbContext<Usuario>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<Producto> Productos { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetalleVenta> DetalleVentas { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de tabla Producto
            modelBuilder.Entity<Producto>(entity =>
            {
                entity.ToTable("productos");
                entity.HasKey(p => p.IdPro);
                entity.Property(p => p.ProductoNombre).IsRequired().HasMaxLength(150);
                entity.Property(p => p.Precio).HasPrecision(18, 2);
                entity.HasIndex(p => p.Activo);
            });

            // Configuración de tabla Venta
            modelBuilder.Entity<Venta>(entity =>
            {
                entity.ToTable("ventas");
                entity.HasKey(v => v.IdVenta);
                entity.Property(v => v.Total).HasPrecision(18, 2);
                
                // Relación con Usuario (Vendedor)
                entity.HasOne(v => v.Vendedor)
                    .WithMany(u => u.Ventas)
                    .HasForeignKey(v => v.VendedorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Configuración de tabla DetalleVenta
            modelBuilder.Entity<DetalleVenta>(entity =>
            {
                entity.ToTable("detalleventas");
                entity.HasKey(d => d.IdDet);
                entity.Property(d => d.Precio).HasPrecision(18, 2);
                entity.Property(d => d.Iva).HasPrecision(18, 2);
                entity.Property(d => d.Total).HasPrecision(18, 2);

                // Relación con Venta
                entity.HasOne(d => d.Venta)
                    .WithMany(v => v.Detalles)
                    .HasForeignKey(d => d.IdVenta)
                    .OnDelete(DeleteBehavior.Cascade);

                // Relación con Producto
                entity.HasOne(d => d.Producto)
                    .WithMany(p => p.DetalleVentas)
                    .HasForeignKey(d => d.IdPro)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<Usuario>().ToTable("usuarios");
        }
    }
}

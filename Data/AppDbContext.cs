using Microsoft.EntityFrameworkCore;
using MantenimientoIndustrial.Models;


namespace MantenimientoIndustrial.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
        public DbSet<Equipo> Equipos { get; set; }
        public DbSet<Mantenimiento> Mantenimientos { get; set; }
        public DbSet<Componente> Componentes { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Alerta> Alertas { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Componente>()
           .HasOne(c => c.Equipo)
           .WithMany(e => e.Componentes)
           .HasForeignKey(c => c.EquipoID);

            base.OnModelCreating(modelBuilder); // Llama al método base

            // Agregar restricciones de unicidad para Codigo y Nombre en Equipos
            modelBuilder.Entity<Equipo>()
                .HasIndex(e => e.Codigo)
                .IsUnique();

            modelBuilder.Entity<Equipo>()
                .HasIndex(e => e.Nombre)
                .IsUnique();
        }
    }
}

using Microsoft.EntityFrameworkCore;
using PharmaGes.API.Models;

namespace PharmaGes.API.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Tablas
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Sesion> Sesiones { get; set; }
        public DbSet<Medicamento> Medicamentos { get; set; }
        public DbSet<MovimientoInventario> MovimientosInventario { get; set; }
        public DbSet<Factura> Facturas { get; set; }
        public DbSet<DetalleFactura> DetallesFactura { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Rol ──────────────────────────────────────────
            modelBuilder.Entity<Rol>(e =>
            {
                e.ToTable("roles");
                e.Property(r => r.Id).HasColumnName("id");
                e.Property(r => r.Nombre).HasColumnName("nombre").HasMaxLength(50).IsRequired();
                e.Property(r => r.EsActivo).HasColumnName("es_activo").HasDefaultValue(true);
                e.Property(r => r.CreadoEn).HasColumnName("creado_en").HasDefaultValueSql("GETDATE()");
            });

            // ── Usuario ──────────────────────────────────────
            modelBuilder.Entity<Usuario>(e =>
            {
                e.ToTable("usuarios");
                e.Property(u => u.Id).HasColumnName("id");
                e.Property(u => u.RolId).HasColumnName("rol_id");
                e.Property(u => u.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
                e.Property(u => u.Email).HasColumnName("email").HasMaxLength(150).IsRequired();
                e.Property(u => u.ContrasenaHash).HasColumnName("contrasena_hash").HasMaxLength(255).IsRequired();
                e.Property(u => u.FotoUrl).HasColumnName("foto_url").HasMaxLength(500);
                e.Property(u => u.EsActivo).HasColumnName("es_activo").HasDefaultValue(true);
                e.Property(u => u.IntentosFallidos).HasColumnName("intentos_fallidos").HasDefaultValue(0);
                e.Property(u => u.BloqueadoHasta).HasColumnName("bloqueado_hasta");
                e.Property(u => u.CreadoEn).HasColumnName("creado_en").HasDefaultValueSql("GETDATE()");
                e.Property(u => u.ActualizadoEn).HasColumnName("actualizado_en").HasDefaultValueSql("GETDATE()");

                e.HasOne(u => u.Rol)
                 .WithMany(r => r.Usuarios)
                 .HasForeignKey(u => u.RolId);
            });

            // ── Sesion ───────────────────────────────────────
            modelBuilder.Entity<Sesion>(e =>
            {
                e.ToTable("sesiones");
                e.Property(s => s.Id).HasColumnName("id");
                e.Property(s => s.UsuarioId).HasColumnName("usuario_id");
                e.Property(s => s.Token).HasColumnName("token").HasMaxLength(500).IsRequired();
                e.Property(s => s.ExpiraEn).HasColumnName("expira_en");
                e.Property(s => s.CreadoEn).HasColumnName("creado_en").HasDefaultValueSql("GETDATE()");

                e.HasOne(s => s.Usuario)
                 .WithMany(u => u.Sesiones)
                 .HasForeignKey(s => s.UsuarioId);
            });

            // ── Medicamento ──────────────────────────────────
            modelBuilder.Entity<Medicamento>(e =>
            {
                e.ToTable("medicamentos");
                e.Property(m => m.Id).HasColumnName("id");
                e.Property(m => m.CreadoPor).HasColumnName("creado_por");
                e.Property(m => m.Codigo).HasColumnName("codigo").HasMaxLength(20).IsRequired();
                e.Property(m => m.Nombre).HasColumnName("nombre").HasMaxLength(150).IsRequired();
                e.Property(m => m.Descripcion).HasColumnName("descripcion").HasMaxLength(500);
                e.Property(m => m.Stock).HasColumnName("stock").HasDefaultValue(0);
                e.Property(m => m.StockMinimo).HasColumnName("stock_minimo").HasDefaultValue(10);
                e.Property(m => m.StockMaximo).HasColumnName("stock_maximo").HasDefaultValue(100);
                e.Property(m => m.PrecioCompra).HasColumnName("precio_compra").HasPrecision(10, 2);
                e.Property(m => m.PrecioVenta).HasColumnName("precio_venta").HasPrecision(10, 2);
                e.Property(m => m.FechaCaducidad).HasColumnName("fecha_caducidad");
                e.Property(m => m.AlertaVencimientoDias).HasColumnName("alerta_vencimiento_dias").HasDefaultValue(30);
                e.Property(m => m.EsActivo).HasColumnName("es_activo").HasDefaultValue(true);
                e.Property(m => m.CreadoEn).HasColumnName("creado_en").HasDefaultValueSql("GETDATE()");
                e.Property(m => m.ActualizadoEn).HasColumnName("actualizado_en").HasDefaultValueSql("GETDATE()");

                e.HasOne(m => m.CreadoPorUsuario)
                 .WithMany(u => u.Medicamentos)
                 .HasForeignKey(m => m.CreadoPor);
            });

            // ── MovimientoInventario ─────────────────────────
            modelBuilder.Entity<MovimientoInventario>(e =>
            {
                e.ToTable("movimientos_inventario");
                e.Property(m => m.Id).HasColumnName("id");
                e.Property(m => m.MedicamentoId).HasColumnName("medicamento_id");
                e.Property(m => m.UsuarioId).HasColumnName("usuario_id");
                e.Property(m => m.Tipo).HasColumnName("tipo").HasMaxLength(20).IsRequired();
                e.Property(m => m.Cantidad).HasColumnName("cantidad");
                e.Property(m => m.StockAnterior).HasColumnName("stock_anterior");
                e.Property(m => m.StockNuevo).HasColumnName("stock_nuevo");
                e.Property(m => m.Motivo).HasColumnName("motivo").HasMaxLength(300);
                e.Property(m => m.CreadoEn).HasColumnName("creado_en").HasDefaultValueSql("GETDATE()");

                e.HasOne(m => m.Medicamento)
                 .WithMany(med => med.Movimientos)
                 .HasForeignKey(m => m.MedicamentoId);

                e.HasOne(m => m.Usuario)
                 .WithMany(u => u.Movimientos)
                 .HasForeignKey(m => m.UsuarioId);
            });

            // ── Factura ──────────────────────────────────────
            modelBuilder.Entity<Factura>(e =>
            {
                e.ToTable("facturas");
                e.Property(f => f.Id).HasColumnName("id");
                e.Property(f => f.UsuarioId).HasColumnName("usuario_id");
                e.Property(f => f.NumeroCorrelativo).HasColumnName("numero_correlativo").HasMaxLength(20).IsRequired();
                e.Property(f => f.Estado).HasColumnName("estado").HasMaxLength(20).HasDefaultValue("activa");
                e.Property(f => f.Subtotal).HasColumnName("subtotal").HasPrecision(10, 2);
                e.Property(f => f.Total).HasColumnName("total").HasPrecision(10, 2);
                e.Property(f => f.EfectivoRecibido).HasColumnName("efectivo_recibido").HasPrecision(10, 2);
                e.Property(f => f.Cambio).HasColumnName("cambio").HasPrecision(10, 2);
                e.Property(f => f.Notas).HasColumnName("notas").HasMaxLength(500);
                e.Property(f => f.CreadoEn).HasColumnName("creado_en").HasDefaultValueSql("GETDATE()");

                e.HasOne(f => f.Usuario)
                 .WithMany(u => u.Facturas)
                 .HasForeignKey(f => f.UsuarioId);
            });

            // ── DetalleFactura ───────────────────────────────
            modelBuilder.Entity<DetalleFactura>(e =>
            {
                e.ToTable("detalles_factura");
                e.Property(d => d.Id).HasColumnName("id");
                e.Property(d => d.FacturaId).HasColumnName("factura_id");
                e.Property(d => d.MedicamentoId).HasColumnName("medicamento_id");
                e.Property(d => d.MedicamentoNombre).HasColumnName("medicamento_nombre").HasMaxLength(150).IsRequired();
                e.Property(d => d.MedicamentoCodigo).HasColumnName("medicamento_codigo").HasMaxLength(20).IsRequired();
                e.Property(d => d.Cantidad).HasColumnName("cantidad");
                e.Property(d => d.PrecioUnitario).HasColumnName("precio_unitario").HasPrecision(10, 2);
                e.Property(d => d.Subtotal).HasColumnName("subtotal").HasPrecision(10, 2)
                 .ValueGeneratedOnAddOrUpdate(); // columna calculada en DB

                e.HasOne(d => d.Factura)
                 .WithMany(f => f.Detalles)
                 .HasForeignKey(d => d.FacturaId);

                e.HasOne(d => d.Medicamento)
                 .WithMany(m => m.DetallesFactura)
                 .HasForeignKey(d => d.MedicamentoId);
            });
        }
    }
}

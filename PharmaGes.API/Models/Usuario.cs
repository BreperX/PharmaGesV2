namespace PharmaGes.API.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public int RolId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ContrasenaHash { get; set; } = string.Empty;
        public string? FotoUrl { get; set; }
        public bool EsActivo { get; set; } = true;
        public int IntentosFallidos { get; set; } = 0;
        public DateTime? BloqueadoHasta { get; set; }
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
        public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;

        // Navegación
        public Rol Rol { get; set; } = null!;
        public ICollection<Sesion> Sesiones { get; set; } = new List<Sesion>();
        public ICollection<Factura> Facturas { get; set; } = new List<Factura>();
        public ICollection<Medicamento> Medicamentos { get; set; } = new List<Medicamento>();
        public ICollection<MovimientoInventario> Movimientos { get; set; } = new List<MovimientoInventario>();
    }
}

namespace PharmaGes.API.Models
{
    public class Factura
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string NumeroCorrelativo { get; set; } = string.Empty;
        public string Estado { get; set; } = "activa"; // activa, anulada
        public decimal Subtotal { get; set; } = 0;
        public decimal Total { get; set; } = 0;
        public decimal EfectivoRecibido { get; set; } = 0;
        public decimal Cambio { get; set; } = 0;
        public string? Notas { get; set; }
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        // Navegación
        public Usuario Usuario { get; set; } = null!;
        public ICollection<DetalleFactura> Detalles { get; set; } = new List<DetalleFactura>();
    }
}

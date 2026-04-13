namespace PharmaGes.API.Models
{
    public class Medicamento
    {
        public int Id { get; set; }
        public int CreadoPor { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int Stock { get; set; } = 0;
        public int StockMinimo { get; set; } = 10;
        public int StockMaximo { get; set; } = 100;
        public decimal PrecioCompra { get; set; } = 0;
        public decimal PrecioVenta { get; set; } = 0;
        public DateOnly? FechaCaducidad { get; set; }
        public int AlertaVencimientoDias { get; set; } = 30;
        public bool EsActivo { get; set; } = true;
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;
        public DateTime ActualizadoEn { get; set; } = DateTime.UtcNow;

        // Navegación
        public Usuario CreadoPorUsuario { get; set; } = null!;
        public ICollection<DetalleFactura> DetallesFactura { get; set; } = new List<DetalleFactura>();
        public ICollection<MovimientoInventario> Movimientos { get; set; } = new List<MovimientoInventario>();
    }
}

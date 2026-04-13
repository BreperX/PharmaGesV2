namespace PharmaGes.API.DTOs
{
    public class CrearFacturaDto
    {
        public decimal EfectivoRecibido { get; set; }
        public string? Notas { get; set; }
        public List<CrearDetalleDto> Detalles { get; set; } = new();
    }

    public class CrearDetalleDto
    {
        public int MedicamentoId { get; set; }
        public int Cantidad { get; set; }
    }

    public class FacturaDto
    {
        public int Id { get; set; }
        public string NumeroCorrelativo { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string UsuarioNombre { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal Total { get; set; }
        public decimal EfectivoRecibido { get; set; }
        public decimal Cambio { get; set; }
        public string? Notas { get; set; }
        public DateTime CreadoEn { get; set; }
        public List<DetalleFacturaDto> Detalles { get; set; } = new();
    }

    public class DetalleFacturaDto
    {
        public string MedicamentoNombre { get; set; } = string.Empty;
        public string MedicamentoCodigo { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}

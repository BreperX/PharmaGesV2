namespace PharmaGes.API.DTOs
{
    public class DashboardDto
    {
        public decimal VentasHoy { get; set; }
        public decimal VentasAyer { get; set; }
        public int ProductosBajoStock { get; set; }
        public int ProductosProximosAVencer { get; set; }
        public List<AlertaStockDto> AlertasStock { get; set; } = new();
        public List<AlertaVencimientoDto> AlertasVencimiento { get; set; } = new();
        public List<VentaRecienteDto> VentasRecientes { get; set; } = new();
    }

    public class AlertaStockDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public int Stock { get; set; }
        public int StockMinimo { get; set; }
        public string Estado { get; set; } = string.Empty; // bajo, agotado
    }

    public class AlertaVencimientoDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public DateOnly FechaCaducidad { get; set; }
        public int DiasParaVencer { get; set; }
    }

    public class VentaRecienteDto
    {
        public string NumeroCorrelativo { get; set; } = string.Empty;
        public decimal Total { get; set; }
        public DateTime CreadoEn { get; set; }
    }

    public class EstadisticasDto
    {
        public decimal VentasTotales { get; set; }
        public decimal PromedioPorVenta { get; set; }
        public int TotalTransacciones { get; set; }
        public List<MedicamentoMasVendidoDto> MasMedVendidos { get; set; } = new();
    }

    public class MedicamentoMasVendidoDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public int CantidadVendida { get; set; }
        public decimal TotalGenerado { get; set; }
    }
}

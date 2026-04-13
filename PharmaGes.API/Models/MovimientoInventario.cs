namespace PharmaGes.API.Models
{
    public class MovimientoInventario
    {
        public int Id { get; set; }
        public int MedicamentoId { get; set; }
        public int UsuarioId { get; set; }
        public string Tipo { get; set; } = string.Empty; // entrada, venta, ajuste, baja
        public int Cantidad { get; set; }
        public int StockAnterior { get; set; }
        public int StockNuevo { get; set; }
        public string? Motivo { get; set; }
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        // Navegación
        public Medicamento Medicamento { get; set; } = null!;
        public Usuario Usuario { get; set; } = null!;
    }
}

namespace PharmaGes.API.Models
{
    public class DetalleFactura
    {
        public int Id { get; set; }
        public int FacturaId { get; set; }
        public int MedicamentoId { get; set; }
        public string MedicamentoNombre { get; set; } = string.Empty;
        public string MedicamentoCodigo { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; } // calculado: Cantidad * PrecioUnitario

        // Navegación
        public Factura Factura { get; set; } = null!;
        public Medicamento Medicamento { get; set; } = null!;
    }
}

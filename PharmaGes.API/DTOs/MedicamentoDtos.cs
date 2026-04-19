namespace PharmaGes.API.DTOs
{
    public class MedicamentoDto
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int Stock { get; set; }
        public int StockMinimo { get; set; }
        public int StockMaximo { get; set; }
        public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; }
        public DateOnly? FechaCaducidad { get; set; }
        public int AlertaVencimientoDias { get; set; }
        public string EstadoStock { get; set; } = string.Empty;
    }

    public class CrearMedicamentoDto
    {
        public string Codigo { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int Stock { get; set; }
        public int StockMinimo { get; set; } = 10;
        public int StockMaximo { get; set; } = 100;
        public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; }
        public DateOnly? FechaCaducidad { get; set; }
        public int AlertaVencimientoDias { get; set; } = 30;
    }

    public class EditarMedicamentoDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int StockMinimo { get; set; }
        public int StockMaximo { get; set; }
        public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; }
        public DateOnly? FechaCaducidad { get; set; }
        public int AlertaVencimientoDias { get; set; }
    }

    public class AjustarStockDto
    {
        /// <summary>entrada = sumar, baja = restar, ajuste = fijar valor absoluto</summary>
        public string Tipo { get; set; } = "entrada";
        public int Cantidad { get; set; }
        public string? Motivo { get; set; }
    }
}

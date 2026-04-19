using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaGes.API.Data;
using PharmaGes.API.DTOs;

namespace PharmaGes.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly AppDbContext _db;

        public DashboardController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetDashboard()
        {
            var hoy  = DateTime.Today;
            var ayer = hoy.AddDays(-1);

            var ventasHoy = await _db.Facturas
                .Where(f => f.Estado == "activa" && f.CreadoEn.Date == hoy)
                .SumAsync(f => (decimal?)f.Total) ?? 0;

            var ventasAyer = await _db.Facturas
                .Where(f => f.Estado == "activa" && f.CreadoEn.Date == ayer)
                .SumAsync(f => (decimal?)f.Total) ?? 0;

            var alertasStock = await _db.Medicamentos
                .Where(m => m.EsActivo && m.Stock <= m.StockMinimo)
                .Select(m => new AlertaStockDto
                {
                    Nombre      = m.Nombre,
                    Codigo      = m.Codigo,
                    Stock       = m.Stock,
                    StockMinimo = m.StockMinimo,
                    Estado      = m.Stock == 0 ? "agotado" : "bajo"
                }).ToListAsync();

            var alertasVencimiento = await _db.Medicamentos
                .Where(m => m.EsActivo && m.FechaCaducidad.HasValue &&
                    m.FechaCaducidad.Value <= DateOnly.FromDateTime(DateTime.Now.AddDays(m.AlertaVencimientoDias)))
                .Select(m => new AlertaVencimientoDto
                {
                    Nombre         = m.Nombre,
                    Codigo         = m.Codigo,
                    FechaCaducidad = m.FechaCaducidad!.Value,
                    DiasParaVencer = (int)(m.FechaCaducidad.Value.ToDateTime(TimeOnly.MinValue) - DateTime.Now).TotalDays
                }).ToListAsync();

            var ventasRecientes = await _db.Facturas
                .Where(f => f.Estado == "activa")
                .OrderByDescending(f => f.CreadoEn)
                .Take(5)
                .Select(f => new VentaRecienteDto
                {
                    NumeroCorrelativo = f.NumeroCorrelativo,
                    Total             = f.Total,
                    CreadoEn          = f.CreadoEn
                }).ToListAsync();

            return Ok(new DashboardDto
            {
                VentasHoy                = ventasHoy,
                VentasAyer               = ventasAyer,
                ProductosBajoStock       = alertasStock.Count,
                ProductosProximosAVencer = alertasVencimiento.Count,
                AlertasStock             = alertasStock,
                AlertasVencimiento       = alertasVencimiento,
                VentasRecientes          = ventasRecientes
            });
        }

        /// <summary>
        /// Ventas de los últimos N días — para la gráfica del dashboard
        /// Rellena con 0 los días sin ventas para que la gráfica sea continua
        /// </summary>
        [HttpGet("ventas-por-dia")]
        public async Task<IActionResult> GetVentasPorDia([FromQuery] int dias = 7)
        {
            var desde = DateTime.Today.AddDays(-(dias - 1));

            var ventas = await _db.Facturas
                .Where(f => f.Estado == "activa" && f.CreadoEn.Date >= desde)
                .GroupBy(f => f.CreadoEn.Date)
                .Select(g => new { Fecha = g.Key, Total = g.Sum(f => f.Total) })
                .ToListAsync();

            var cultura    = new System.Globalization.CultureInfo("es-CO");
            var resultado  = Enumerable.Range(0, dias).Select(i =>
            {
                var fecha = desde.AddDays(i);
                var venta = ventas.FirstOrDefault(v => v.Fecha == fecha);
                return new
                {
                    fecha = fecha.ToString("yyyy-MM-dd"),
                    label = fecha.ToString("ddd", cultura),
                    total = venta?.Total ?? 0
                };
            }).ToList();

            return Ok(resultado);
        }

        [HttpGet("estadisticas")]
        [Authorize(Roles = "Administrador,Gerente")]
        public async Task<IActionResult> GetEstadisticas(
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta)
        {
            var query = _db.Facturas.Where(f => f.Estado == "activa");
            if (desde.HasValue) query = query.Where(f => f.CreadoEn >= desde);
            if (hasta.HasValue) query = query.Where(f => f.CreadoEn <= hasta.Value.AddDays(1));

            var total         = await query.SumAsync(f => (decimal?)f.Total) ?? 0;
            var transacciones = await query.CountAsync();
            var promedio      = transacciones > 0 ? total / transacciones : 0;

            var masVendidos = await _db.DetallesFactura
                .Include(d => d.Factura)
                .Where(d => d.Factura.Estado == "activa" &&
                    (!desde.HasValue || d.Factura.CreadoEn >= desde) &&
                    (!hasta.HasValue || d.Factura.CreadoEn <= hasta.Value.AddDays(1)))
                .GroupBy(d => new { d.MedicamentoNombre, d.MedicamentoCodigo })
                .Select(g => new MedicamentoMasVendidoDto
                {
                    Nombre          = g.Key.MedicamentoNombre,
                    Codigo          = g.Key.MedicamentoCodigo,
                    CantidadVendida = g.Sum(d => d.Cantidad),
                    TotalGenerado   = g.Sum(d => d.Subtotal)
                })
                .OrderByDescending(m => m.CantidadVendida)
                .Take(10)
                .ToListAsync();

            return Ok(new EstadisticasDto
            {
                VentasTotales      = total,
                PromedioPorVenta   = promedio,
                TotalTransacciones = transacciones,
                MasMedVendidos     = masVendidos
            });
        }
    }
}

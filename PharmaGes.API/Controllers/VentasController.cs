using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaGes.API.Data;
using PharmaGes.API.DTOs;
using PharmaGes.API.Models;
using System.Security.Claims;

namespace PharmaGes.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VentasController : ControllerBase
    {
        private readonly AppDbContext _db;

        public VentasController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [Authorize(Roles = "Administrador,Gerente")]
        public async Task<IActionResult> GetTodas(
            [FromQuery] DateTime? desde,
            [FromQuery] DateTime? hasta)
        {
            var query = _db.Facturas
                .Include(f => f.Usuario)
                .Include(f => f.Detalles)
                .Where(f => f.Estado == "activa");

            if (desde.HasValue) query = query.Where(f => f.CreadoEn >= desde);
            if (hasta.HasValue) query = query.Where(f => f.CreadoEn <= hasta.Value.AddDays(1));

            var facturas = await query.OrderByDescending(f => f.CreadoEn)
                .Select(f => new FacturaDto
                {
                    Id                = f.Id,
                    NumeroCorrelativo = f.NumeroCorrelativo,
                    Estado            = f.Estado,
                    UsuarioNombre     = f.Usuario.Nombre,
                    Subtotal          = f.Subtotal,
                    Total             = f.Total,
                    EfectivoRecibido  = f.EfectivoRecibido,
                    Cambio            = f.Cambio,
                    Notas             = f.Notas,
                    CreadoEn          = f.CreadoEn,
                    Detalles          = f.Detalles.Select(d => new DetalleFacturaDto
                    {
                        MedicamentoNombre = d.MedicamentoNombre,
                        MedicamentoCodigo = d.MedicamentoCodigo,
                        Cantidad          = d.Cantidad,
                        PrecioUnitario    = d.PrecioUnitario,
                        Subtotal          = d.Subtotal
                    }).ToList()
                }).ToListAsync();

            return Ok(facturas);
        }

        [HttpPost]
        public async Task<IActionResult> Crear([FromBody] CrearFacturaDto dto)
        {
            if (dto.Detalles == null || !dto.Detalles.Any())
                return BadRequest(new { mensaje = "La factura debe tener al menos un producto." });

            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Validar stock y obtener medicamentos
            var detalles = new List<DetalleFactura>();
            decimal subtotal = 0;

            foreach (var item in dto.Detalles)
            {
                var med = await _db.Medicamentos.FindAsync(item.MedicamentoId);
                if (med == null || !med.EsActivo)
                    return BadRequest(new { mensaje = $"Medicamento ID {item.MedicamentoId} no encontrado." });

                if (med.Stock < item.Cantidad)
                    return BadRequest(new { mensaje = $"Stock insuficiente para {med.Nombre}. Disponible: {med.Stock}" });

                var lineaSubtotal = med.PrecioVenta * item.Cantidad;
                subtotal += lineaSubtotal;

                detalles.Add(new DetalleFactura
                {
                    MedicamentoId     = med.Id,
                    MedicamentoNombre = med.Nombre,
                    MedicamentoCodigo = med.Codigo,
                    Cantidad          = item.Cantidad,
                    PrecioUnitario    = med.PrecioVenta,
                    Subtotal          = lineaSubtotal
                });
            }

            if (dto.EfectivoRecibido < subtotal)
                return BadRequest(new { mensaje = "Efectivo insuficiente para cubrir el total." });

            // Generar número correlativo
            var ultimoNum = await _db.Facturas
                .OrderByDescending(f => f.Id)
                .Select(f => f.NumeroCorrelativo)
                .FirstOrDefaultAsync();

            int siguiente = 1;
            if (ultimoNum != null)
                siguiente = int.Parse(ultimoNum.Replace("F-", "")) + 1;

            var correlativo = $"F-{siguiente:D5}";

            // Crear factura
            var factura = new Factura
            {
                UsuarioId        = usuarioId,
                NumeroCorrelativo = correlativo,
                Estado           = "activa",
                Subtotal         = subtotal,
                Total            = subtotal,
                EfectivoRecibido = dto.EfectivoRecibido,
                Cambio           = dto.EfectivoRecibido - subtotal,
                Notas            = dto.Notas,
                Detalles         = detalles
            };

            _db.Facturas.Add(factura);

            // Descontar stock y registrar movimientos
            foreach (var item in dto.Detalles)
            {
                var med = await _db.Medicamentos.FindAsync(item.MedicamentoId);
                var stockAnterior = med!.Stock;
                med.Stock -= item.Cantidad;
                med.ActualizadoEn = DateTime.UtcNow;

                _db.MovimientosInventario.Add(new MovimientoInventario
                {
                    MedicamentoId = med.Id,
                    UsuarioId     = usuarioId,
                    Tipo          = "venta",
                    Cantidad      = item.Cantidad,
                    StockAnterior = stockAnterior,
                    StockNuevo    = med.Stock,
                    Motivo        = $"Venta {correlativo}"
                });
            }

            await _db.SaveChangesAsync();

            return Ok(new { mensaje = "Venta registrada correctamente.", correlativo, total = subtotal, cambio = dto.EfectivoRecibido - subtotal });
        }

        [HttpPut("{id}/anular")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Anular(int id, [FromBody] string? motivo)
        {
            var factura = await _db.Facturas.Include(f => f.Detalles).FirstOrDefaultAsync(f => f.Id == id);
            if (factura == null) return NotFound(new { mensaje = "Factura no encontrada." });
            if (factura.Estado == "anulada") return BadRequest(new { mensaje = "La factura ya está anulada." });

            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            // Revertir stock
            foreach (var detalle in factura.Detalles)
            {
                var med = await _db.Medicamentos.FindAsync(detalle.MedicamentoId);
                if (med != null)
                {
                    var stockAnterior = med.Stock;
                    med.Stock += detalle.Cantidad;
                    med.ActualizadoEn = DateTime.UtcNow;

                    _db.MovimientosInventario.Add(new MovimientoInventario
                    {
                        MedicamentoId = med.Id,
                        UsuarioId     = usuarioId,
                        Tipo          = "ajuste",
                        Cantidad      = detalle.Cantidad,
                        StockAnterior = stockAnterior,
                        StockNuevo    = med.Stock,
                        Motivo        = $"Anulación factura {factura.NumeroCorrelativo}"
                    });
                }
            }

            factura.Estado = "anulada";
            factura.Notas  = motivo ?? factura.Notas;
            await _db.SaveChangesAsync();

            return Ok(new { mensaje = "Factura anulada correctamente." });
        }
    }
}

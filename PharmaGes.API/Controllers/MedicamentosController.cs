using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaGes.API.Data;
using PharmaGes.API.DTOs;
using PharmaGes.API.Models;
using System.Security.Claims;
using System.Text.RegularExpressions;

namespace PharmaGes.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class MedicamentosController : ControllerBase
    {
        private readonly AppDbContext _db;

        public MedicamentosController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<IActionResult> GetTodos(
            [FromQuery] string? busqueda,
            [FromQuery] decimal? precioMin,
            [FromQuery] decimal? precioMax,
            [FromQuery] int? stockMin,
            [FromQuery] int? stockMax,
            [FromQuery] bool? proximosAVencer,
            [FromQuery] bool? proximosAAgotarse)
        {
            var query = _db.Medicamentos.Where(m => m.EsActivo);

            if (!string.IsNullOrWhiteSpace(busqueda))
                query = query.Where(m => m.Nombre.Contains(busqueda) || m.Codigo.Contains(busqueda));

            if (precioMin.HasValue) query = query.Where(m => m.PrecioVenta >= precioMin);
            if (precioMax.HasValue) query = query.Where(m => m.PrecioVenta <= precioMax);
            if (stockMin.HasValue)  query = query.Where(m => m.Stock >= stockMin);
            if (stockMax.HasValue)  query = query.Where(m => m.Stock <= stockMax);

            if (proximosAVencer == true)
                query = query.Where(m => m.FechaCaducidad.HasValue &&
                    m.FechaCaducidad.Value <= DateOnly.FromDateTime(DateTime.Now.AddDays(m.AlertaVencimientoDias)));

            if (proximosAAgotarse == true)
                query = query.Where(m => m.Stock <= m.StockMinimo);

            var medicamentos = await query.Select(m => new MedicamentoDto
            {
                Id                    = m.Id,
                Codigo                = m.Codigo,
                Nombre                = m.Nombre,
                Descripcion           = m.Descripcion,
                Stock                 = m.Stock,
                StockMinimo           = m.StockMinimo,
                StockMaximo           = m.StockMaximo,
                PrecioCompra          = m.PrecioCompra,
                PrecioVenta           = m.PrecioVenta,
                FechaCaducidad        = m.FechaCaducidad,
                AlertaVencimientoDias = m.AlertaVencimientoDias,
                EstadoStock           = m.Stock == 0 ? "agotado" : m.Stock <= m.StockMinimo ? "bajo" : "ok"
            }).ToListAsync();

            return Ok(medicamentos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var m = await _db.Medicamentos.FirstOrDefaultAsync(m => m.Id == id && m.EsActivo);
            if (m == null) return NotFound(new { mensaje = "Medicamento no encontrado." });

            return Ok(new MedicamentoDto
            {
                Id                    = m.Id,
                Codigo                = m.Codigo,
                Nombre                = m.Nombre,
                Descripcion           = m.Descripcion,
                Stock                 = m.Stock,
                StockMinimo           = m.StockMinimo,
                StockMaximo           = m.StockMaximo,
                PrecioCompra          = m.PrecioCompra,
                PrecioVenta           = m.PrecioVenta,
                FechaCaducidad        = m.FechaCaducidad,
                AlertaVencimientoDias = m.AlertaVencimientoDias,
                EstadoStock           = m.Stock == 0 ? "agotado" : m.Stock <= m.StockMinimo ? "bajo" : "ok"
            });
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Gerente")]
        public async Task<IActionResult> Crear([FromBody] CrearMedicamentoDto dto)
        {
            // Validar código — solo letras, números y guiones
            if (!Regex.IsMatch(dto.Codigo, @"^[a-zA-Z0-9\-]+$"))
                return BadRequest(new { mensaje = "El código solo puede contener letras, números y guiones." });

            if (await _db.Medicamentos.AnyAsync(m => m.Codigo == dto.Codigo))
                return BadRequest(new { mensaje = "Ya existe un medicamento con ese código." });

            // Validar stock no negativo
            if (dto.Stock < 0)
                return BadRequest(new { mensaje = "El stock no puede ser negativo." });

            if (dto.StockMinimo < 0)
                return BadRequest(new { mensaje = "El stock mínimo no puede ser negativo." });

            if (dto.StockMaximo < dto.StockMinimo)
                return BadRequest(new { mensaje = "El stock máximo no puede ser menor que el mínimo." });

            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var medicamento = new Medicamento
            {
                CreadoPor             = usuarioId,
                Codigo                = dto.Codigo.ToUpper(),
                Nombre                = dto.Nombre,
                Descripcion           = dto.Descripcion,
                Stock                 = dto.Stock,
                StockMinimo           = dto.StockMinimo,
                StockMaximo           = dto.StockMaximo,
                PrecioCompra          = dto.PrecioCompra,
                PrecioVenta           = dto.PrecioVenta,
                FechaCaducidad        = dto.FechaCaducidad,
                AlertaVencimientoDias = dto.AlertaVencimientoDias
            };

            _db.Medicamentos.Add(medicamento);
            await _db.SaveChangesAsync();

            if (dto.Stock > 0)
            {
                _db.MovimientosInventario.Add(new MovimientoInventario
                {
                    MedicamentoId = medicamento.Id,
                    UsuarioId     = usuarioId,
                    Tipo          = "entrada",
                    Cantidad      = dto.Stock,
                    StockAnterior = 0,
                    StockNuevo    = dto.Stock,
                    Motivo        = "Ingreso inicial de medicamento"
                });
                await _db.SaveChangesAsync();
            }

            return Ok(new { mensaje = "Medicamento creado correctamente.", id = medicamento.Id });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador,Gerente")]
        public async Task<IActionResult> Editar(int id, [FromBody] EditarMedicamentoDto dto)
        {
            var medicamento = await _db.Medicamentos.FindAsync(id);
            if (medicamento == null || !medicamento.EsActivo)
                return NotFound(new { mensaje = "Medicamento no encontrado." });

            if (dto.StockMinimo < 0)
                return BadRequest(new { mensaje = "El stock mínimo no puede ser negativo." });

            if (dto.StockMaximo < dto.StockMinimo)
                return BadRequest(new { mensaje = "El stock máximo no puede ser menor que el mínimo." });

            medicamento.Nombre                = dto.Nombre;
            medicamento.Descripcion           = dto.Descripcion;
            medicamento.StockMinimo           = dto.StockMinimo;
            medicamento.StockMaximo           = dto.StockMaximo;
            medicamento.PrecioCompra          = dto.PrecioCompra;
            medicamento.PrecioVenta           = dto.PrecioVenta;
            medicamento.FechaCaducidad        = dto.FechaCaducidad;
            medicamento.AlertaVencimientoDias = dto.AlertaVencimientoDias;
            medicamento.ActualizadoEn         = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok(new { mensaje = "Medicamento actualizado correctamente." });
        }

        /// <summary>
        /// Reponer o ajustar stock — registra movimiento con quien lo hizo
        /// tipo: "entrada" para agregar, "ajuste" para corrección, "baja" para retirar
        /// </summary>
        [HttpPost("{id}/stock")]
        [Authorize(Roles = "Administrador,Gerente")]
        public async Task<IActionResult> AjustarStock(int id, [FromBody] AjustarStockDto dto)
        {
            var medicamento = await _db.Medicamentos.FindAsync(id);
            if (medicamento == null || !medicamento.EsActivo)
                return NotFound(new { mensaje = "Medicamento no encontrado." });

            var tiposValidos = new[] { "entrada", "ajuste", "baja" };
            if (!tiposValidos.Contains(dto.Tipo))
                return BadRequest(new { mensaje = "Tipo inválido. Use: entrada, ajuste o baja." });

            if (dto.Cantidad <= 0)
                return BadRequest(new { mensaje = "La cantidad debe ser mayor a 0." });

            var stockAnterior = medicamento.Stock;
            int stockNuevo;

            if (dto.Tipo == "entrada")
            {
                stockNuevo = stockAnterior + dto.Cantidad;
            }
            else if (dto.Tipo == "baja")
            {
                stockNuevo = stockAnterior - dto.Cantidad;
                if (stockNuevo < 0)
                    return BadRequest(new { mensaje = $"No hay suficiente stock. Stock actual: {stockAnterior}" });
            }
            else // ajuste
            {
                stockNuevo = dto.Cantidad; // en ajuste, la cantidad ES el nuevo stock total
                if (stockNuevo < 0)
                    return BadRequest(new { mensaje = "El stock ajustado no puede ser negativo." });
            }

            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            medicamento.Stock         = stockNuevo;
            medicamento.ActualizadoEn = DateTime.UtcNow;

            _db.MovimientosInventario.Add(new MovimientoInventario
            {
                MedicamentoId = medicamento.Id,
                UsuarioId     = usuarioId,
                Tipo          = dto.Tipo,
                Cantidad      = dto.Cantidad,
                StockAnterior = stockAnterior,
                StockNuevo    = stockNuevo,
                Motivo        = dto.Motivo ?? $"{dto.Tipo} manual"
            });

            await _db.SaveChangesAsync();

            return Ok(new
            {
                mensaje       = "Stock actualizado correctamente.",
                stockAnterior,
                stockNuevo
            });
        }

        [HttpGet("{id}/movimientos")]
        [Authorize(Roles = "Administrador,Gerente")]
        public async Task<IActionResult> GetMovimientos(int id)
        {
            var movimientos = await _db.MovimientosInventario
                .Include(m => m.Usuario)
                .Where(m => m.MedicamentoId == id)
                .OrderByDescending(m => m.CreadoEn)
                .Select(m => new
                {
                    m.Tipo,
                    m.Cantidad,
                    m.StockAnterior,
                    m.StockNuevo,
                    m.Motivo,
                    m.CreadoEn,
                    Usuario = m.Usuario.Nombre
                })
                .ToListAsync();

            return Ok(movimientos);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador,Gerente")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var medicamento = await _db.Medicamentos.FindAsync(id);
            if (medicamento == null || !medicamento.EsActivo)
                return NotFound(new { mensaje = "Medicamento no encontrado." });

            medicamento.EsActivo      = false;
            medicamento.ActualizadoEn = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Ok(new { mensaje = "Medicamento eliminado correctamente." });
        }
    }
}

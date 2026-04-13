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
                Id                   = m.Id,
                Codigo               = m.Codigo,
                Nombre               = m.Nombre,
                Descripcion          = m.Descripcion,
                Stock                = m.Stock,
                StockMinimo          = m.StockMinimo,
                StockMaximo          = m.StockMaximo,
                PrecioCompra         = m.PrecioCompra,
                PrecioVenta          = m.PrecioVenta,
                FechaCaducidad       = m.FechaCaducidad,
                AlertaVencimientoDias = m.AlertaVencimientoDias,
                EstadoStock          = m.Stock == 0 ? "agotado" : m.Stock <= m.StockMinimo ? "bajo" : "ok"
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
                Id                   = m.Id,
                Codigo               = m.Codigo,
                Nombre               = m.Nombre,
                Descripcion          = m.Descripcion,
                Stock                = m.Stock,
                StockMinimo          = m.StockMinimo,
                StockMaximo          = m.StockMaximo,
                PrecioCompra         = m.PrecioCompra,
                PrecioVenta          = m.PrecioVenta,
                FechaCaducidad       = m.FechaCaducidad,
                AlertaVencimientoDias = m.AlertaVencimientoDias,
                EstadoStock          = m.Stock == 0 ? "agotado" : m.Stock <= m.StockMinimo ? "bajo" : "ok"
            });
        }

        [HttpPost]
        [Authorize(Roles = "Administrador,Gerente")]
        public async Task<IActionResult> Crear([FromBody] CrearMedicamentoDto dto)
        {
            if (await _db.Medicamentos.AnyAsync(m => m.Codigo == dto.Codigo))
                return BadRequest(new { mensaje = "Ya existe un medicamento con ese código." });

            var usuarioId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var medicamento = new Medicamento
            {
                CreadoPor            = usuarioId,
                Codigo               = dto.Codigo,
                Nombre               = dto.Nombre,
                Descripcion          = dto.Descripcion,
                Stock                = dto.Stock,
                StockMinimo          = dto.StockMinimo,
                StockMaximo          = dto.StockMaximo,
                PrecioCompra         = dto.PrecioCompra,
                PrecioVenta          = dto.PrecioVenta,
                FechaCaducidad       = dto.FechaCaducidad,
                AlertaVencimientoDias = dto.AlertaVencimientoDias
            };

            _db.Medicamentos.Add(medicamento);
            await _db.SaveChangesAsync();

            // Registrar movimiento de entrada inicial
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
            return Ok(new { mensaje = "Medicamento creado correctamente.", id = medicamento.Id });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador,Gerente")]
        public async Task<IActionResult> Editar(int id, [FromBody] EditarMedicamentoDto dto)
        {
            var medicamento = await _db.Medicamentos.FindAsync(id);
            if (medicamento == null || !medicamento.EsActivo)
                return NotFound(new { mensaje = "Medicamento no encontrado." });

            medicamento.Nombre               = dto.Nombre;
            medicamento.Descripcion          = dto.Descripcion;
            medicamento.StockMinimo          = dto.StockMinimo;
            medicamento.StockMaximo          = dto.StockMaximo;
            medicamento.PrecioCompra         = dto.PrecioCompra;
            medicamento.PrecioVenta          = dto.PrecioVenta;
            medicamento.FechaCaducidad       = dto.FechaCaducidad;
            medicamento.AlertaVencimientoDias = dto.AlertaVencimientoDias;
            medicamento.ActualizadoEn        = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok(new { mensaje = "Medicamento actualizado correctamente." });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador,Gerente")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var medicamento = await _db.Medicamentos.FindAsync(id);
            if (medicamento == null || !medicamento.EsActivo)
                return NotFound(new { mensaje = "Medicamento no encontrado." });

            // Soft delete
            medicamento.EsActivo      = false;
            medicamento.ActualizadoEn = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Ok(new { mensaje = "Medicamento eliminado correctamente." });
        }
    }
}

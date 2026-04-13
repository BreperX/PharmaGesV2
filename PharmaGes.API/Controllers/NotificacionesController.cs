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
    public class NotificacionesController : ControllerBase
    {
        private readonly AppDbContext _db;

        public NotificacionesController(AppDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Devuelve todas las alertas activas:
        /// - Medicamentos con stock <= stock_minimo
        /// - Medicamentos que vencen dentro de sus dias de alerta configurados
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetNotificaciones()
        {
            var hoy = DateOnly.FromDateTime(DateTime.Now);

            var alertasStock = await _db.Medicamentos
                .Where(m => m.EsActivo && m.Stock <= m.StockMinimo)
                .Select(m => new AlertaStockDto
                {
                    Nombre      = m.Nombre,
                    Codigo      = m.Codigo,
                    Stock       = m.Stock,
                    StockMinimo = m.StockMinimo,
                    Estado      = m.Stock == 0 ? "agotado" : "bajo"
                })
                .OrderBy(m => m.Stock)
                .ToListAsync();

            var alertasVencimiento = await _db.Medicamentos
                .Where(m => m.EsActivo
                    && m.FechaCaducidad.HasValue
                    && m.FechaCaducidad.Value <= hoy.AddDays(m.AlertaVencimientoDias))
                .Select(m => new AlertaVencimientoDto
                {
                    Nombre         = m.Nombre,
                    Codigo         = m.Codigo,
                    FechaCaducidad = m.FechaCaducidad!.Value,
                    DiasParaVencer = (int)(m.FechaCaducidad.Value.ToDateTime(TimeOnly.MinValue) - DateTime.Now).TotalDays
                })
                .OrderBy(m => m.DiasParaVencer)
                .ToListAsync();

            return Ok(new
            {
                totalAlertas          = alertasStock.Count + alertasVencimiento.Count,
                alertasStock,
                alertasVencimiento
            });
        }

        /// <summary>
        /// Resumen rápido solo con conteos — para el badge del header
        /// </summary>
        [HttpGet("resumen")]
        public async Task<IActionResult> GetResumen()
        {
            var hoy = DateOnly.FromDateTime(DateTime.Now);

            var stockBajo = await _db.Medicamentos
                .CountAsync(m => m.EsActivo && m.Stock <= m.StockMinimo);

            var proximosVencer = await _db.Medicamentos
                .CountAsync(m => m.EsActivo
                    && m.FechaCaducidad.HasValue
                    && m.FechaCaducidad.Value <= hoy.AddDays(m.AlertaVencimientoDias));

            return Ok(new
            {
                stockBajo,
                proximosVencer,
                total = stockBajo + proximosVencer
            });
        }
    }
}

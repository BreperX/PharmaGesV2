using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaGes.API.Data;
using PharmaGes.API.DTOs;
using PharmaGes.API.Helpers;
using PharmaGes.API.Services;

namespace PharmaGes.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly JwtHelper _jwt;
        private readonly TimeZoneService _tz;

        public AuthController(AppDbContext db, JwtHelper jwt, TimeZoneService tz)
        {
            _db  = db;
            _jwt = jwt;
            _tz  = tz;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            var usuario = await _db.Usuarios
                .Include(u => u.Rol)
                .FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.ToLower() && u.EsActivo);

            if (usuario == null)
                return Unauthorized(new { mensaje = "Credenciales incorrectas." });

            // Verificar si está bloqueado — comparar contra hora local del cliente
            if (usuario.BloqueadoHasta.HasValue && usuario.BloqueadoHasta > _tz.Ahora())
            {
                var minutosRestantes = (int)(usuario.BloqueadoHasta.Value - _tz.Ahora()).TotalMinutes + 1;
                return Unauthorized(new { mensaje = $"Usuario bloqueado. Intenta en {minutosRestantes} minuto(s)." });
            }

            // Verificar contraseña
            if (!BCrypt.Net.BCrypt.Verify(dto.Contrasena, usuario.ContrasenaHash))
            {
                usuario.IntentosFallidos++;

                if (usuario.IntentosFallidos >= 5)
                {
                    int minutos = (int)Math.Pow(2, usuario.IntentosFallidos / 5);
                    usuario.BloqueadoHasta = _tz.Ahora().AddMinutes(minutos);
                }

                await _db.SaveChangesAsync();

                int restantes = 5 - (usuario.IntentosFallidos % 5);
                return Unauthorized(new { mensaje = $"Credenciales incorrectas. Te quedan {restantes} intento(s)." });
            }

            // Login exitoso — resetear intentos
            usuario.IntentosFallidos = 0;
            usuario.BloqueadoHasta   = null;
            await _db.SaveChangesAsync();

            var (token, expira) = _jwt.GenerarToken(usuario, usuario.Rol.Nombre);

            return Ok(new LoginResponseDto
            {
                Token    = token,
                Nombre   = usuario.Nombre,
                Email    = usuario.Email,
                Rol      = usuario.Rol.Nombre,
                FotoUrl  = usuario.FotoUrl,
                ExpiraEn = expira
            });
        }
    }
}

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaGes.API.Data;
using PharmaGes.API.DTOs;
using PharmaGes.API.Models;
using PharmaGes.API.Services;
using System.Security.Claims;

namespace PharmaGes.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly TimeZoneService _tz;

        public UsuariosController(AppDbContext db, TimeZoneService tz)
        {
            _db = db;
            _tz = tz;
        }

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> GetTodos([FromQuery] string? busqueda)
        {
            var query = _db.Usuarios.Include(u => u.Rol).Where(u => u.EsActivo);

            if (!string.IsNullOrWhiteSpace(busqueda))
            {
                var b = busqueda.ToLower();
                query = query.Where(u => u.Nombre.ToLower().Contains(b) ||
                                         u.Apellido.ToLower().Contains(b) ||
                                         u.Email.ToLower().Contains(b));
            }

            var usuarios = await query.Select(u => new UsuarioDto
            {
                Id       = u.Id,
                Nombre   = u.Nombre,
                Apellido = u.Apellido,
                Email    = u.Email,
                Rol      = u.Rol.Nombre,
                FotoUrl  = u.FotoUrl,
                EsActivo = u.EsActivo,
                CreadoEn = u.CreadoEn
            }).ToListAsync();

            return Ok(usuarios);
        }

        [HttpGet("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> GetById(int id)
        {
            var u = await _db.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Id == id);
            if (u == null) return NotFound(new { mensaje = "Usuario no encontrado." });

            return Ok(new UsuarioDto
            {
                Id       = u.Id,
                Nombre   = u.Nombre,
                Apellido = u.Apellido,
                Email    = u.Email,
                Rol      = u.Rol.Nombre,
                FotoUrl  = u.FotoUrl,
                EsActivo = u.EsActivo,
                CreadoEn = u.CreadoEn
            });
        }

        [HttpPost]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Crear([FromBody] CrearUsuarioDto dto)
        {
            if (await _db.Usuarios.AnyAsync(u => u.Email.ToLower() == dto.Email.ToLower()))
                return BadRequest(new { mensaje = "Ya existe un usuario con ese correo." });

            var usuario = new Usuario
            {
                RolId          = dto.RolId,
                Nombre         = dto.Nombre,
                Apellido       = dto.Apellido,
                Email          = dto.Email,
                ContrasenaHash = BCrypt.Net.BCrypt.HashPassword(dto.Contrasena),
                FotoUrl        = dto.FotoUrl,
                CreadoEn       = _tz.Ahora()
            };

            _db.Usuarios.Add(usuario);
            await _db.SaveChangesAsync();

            return Ok(new { mensaje = "Usuario creado correctamente.", id = usuario.Id });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Editar(int id, [FromBody] EditarUsuarioDto dto)
        {
            var usuario = await _db.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound(new { mensaje = "Usuario no encontrado." });

            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            if (id == currentUserId && !dto.EsActivo)
                return BadRequest(new { mensaje = "No puedes desactivar tu propia cuenta." });

            usuario.Nombre        = dto.Nombre;
            usuario.Apellido      = dto.Apellido;
            usuario.Email         = dto.Email;
            usuario.RolId         = dto.RolId;
            usuario.FotoUrl       = dto.FotoUrl;
            usuario.EsActivo      = dto.EsActivo;
            usuario.ActualizadoEn = _tz.Ahora();

            if (!string.IsNullOrWhiteSpace(dto.Contrasena))
                usuario.ContrasenaHash = BCrypt.Net.BCrypt.HashPassword(dto.Contrasena);

            await _db.SaveChangesAsync();
            return Ok(new { mensaje = "Usuario actualizado correctamente." });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var currentUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            if (id == currentUserId)
                return BadRequest(new { mensaje = "Operación denegada: No puedes eliminar la cuenta con la que has iniciado sesión." });

            var usuario = await _db.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound(new { mensaje = "Usuario no encontrado." });

            usuario.Nombre         = "Usuario";
            usuario.Apellido       = "Eliminado";
            usuario.Email          = $"eliminado_{id}@pharmages.local";
            usuario.ContrasenaHash = "ANONYMIZED_ACCOUNT";
            usuario.FotoUrl        = null;
            usuario.EsActivo       = false;
            usuario.ActualizadoEn  = _tz.Ahora();

            await _db.SaveChangesAsync();

            return Ok(new { mensaje = "Usuario eliminado y correo liberado correctamente." });
        }

        [HttpGet("roles")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> GetRoles()
        {
            var roles = await _db.Roles.Where(r => r.EsActivo)
                .Select(r => new { r.Id, r.Nombre })
                .ToListAsync();
            return Ok(roles);
        }
    }
}

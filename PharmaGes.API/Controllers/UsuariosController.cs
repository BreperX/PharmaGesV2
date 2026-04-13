using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PharmaGes.API.Data;
using PharmaGes.API.DTOs;
using PharmaGes.API.Models;

namespace PharmaGes.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        private readonly AppDbContext _db;

        public UsuariosController(AppDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> GetTodos([FromQuery] string? busqueda)
        {
            var query = _db.Usuarios.Include(u => u.Rol).Where(u => u.EsActivo);

            if (!string.IsNullOrWhiteSpace(busqueda))
                query = query.Where(u => u.Nombre.Contains(busqueda) || u.Email.Contains(busqueda));

            var usuarios = await query.Select(u => new UsuarioDto
            {
                Id       = u.Id,
                Nombre   = u.Nombre,
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
                RolId         = dto.RolId,
                Nombre        = dto.Nombre,
                Email         = dto.Email,
                ContrasenaHash = BCrypt.Net.BCrypt.HashPassword(dto.Contrasena),
                FotoUrl       = dto.FotoUrl
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

            usuario.Nombre        = dto.Nombre;
            usuario.Email         = dto.Email;
            usuario.RolId         = dto.RolId;
            usuario.FotoUrl       = dto.FotoUrl;
            usuario.EsActivo      = dto.EsActivo;
            usuario.ActualizadoEn = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(dto.Contrasena))
                usuario.ContrasenaHash = BCrypt.Net.BCrypt.HashPassword(dto.Contrasena);

            await _db.SaveChangesAsync();
            return Ok(new { mensaje = "Usuario actualizado correctamente." });
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Eliminar(int id)
        {
            var usuario = await _db.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound(new { mensaje = "Usuario no encontrado." });

            // Soft delete
            usuario.EsActivo      = false;
            usuario.ActualizadoEn = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Ok(new { mensaje = "Usuario desactivado correctamente." });
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

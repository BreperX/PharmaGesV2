namespace PharmaGes.API.DTOs
{
    public class UsuarioDto
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
        public string? FotoUrl { get; set; }
        public bool EsActivo { get; set; }
        public DateTime CreadoEn { get; set; }
    }

    public class CrearUsuarioDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Contrasena { get; set; } = string.Empty;
        public int RolId { get; set; }
        public string? FotoUrl { get; set; }
    }

    public class EditarUsuarioDto
    {
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Contrasena { get; set; } 
        public int RolId { get; set; }
        public string? FotoUrl { get; set; }
        public bool EsActivo { get; set; }
    }
}
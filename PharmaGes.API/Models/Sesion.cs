namespace PharmaGes.API.Models
{
    public class Sesion
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public string Token { get; set; } = string.Empty;
        public DateTime ExpiraEn { get; set; }
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        // Navegación
        public Usuario Usuario { get; set; } = null!;
    }
}

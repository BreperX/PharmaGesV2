namespace PharmaGes.API.Models
{
    public class Rol
    {
        public int Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public bool EsActivo { get; set; } = true;
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        // Navegación
        public ICollection<Usuario> Usuarios { get; set; } = new List<Usuario>();
    }
}

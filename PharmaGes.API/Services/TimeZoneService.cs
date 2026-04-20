namespace PharmaGes.API.Services
{
    /// <summary>
    /// Lee el offset UTC del cliente desde el header "X-Timezone-Offset"
    /// (valor en minutos, igual que JS Date.getTimezoneOffset()).
    /// Colombia = 300 minutos = UTC-5.
    /// Si el header no llega, usa UTC-5 como fallback.
    /// </summary>
    public class TimeZoneService
    {
        private readonly int _offsetMinutos;

        public TimeZoneService(IHttpContextAccessor httpContextAccessor)
        {
            var header = httpContextAccessor.HttpContext?
                .Request.Headers["X-Timezone-Offset"].FirstOrDefault();

            // JS devuelve el offset invertido: Colombia = 300 (UTC-5)
            // Para ajustar: DateTime.UtcNow - offsetMinutos = hora local
            if (int.TryParse(header, out var parsed))
                _offsetMinutos = parsed;
            else
                _offsetMinutos = 300; // fallback Colombia UTC-5
        }

        /// <summary>
        /// Hora actual en la zona del cliente.
        /// </summary>
        public DateTime Ahora() =>
            DateTime.UtcNow.AddMinutes(-_offsetMinutos);

        /// <summary>
        /// Convierte un DateTime UTC a la zona del cliente.
        /// </summary>
        public DateTime Convertir(DateTime utc) =>
            utc.AddMinutes(-_offsetMinutos);
    }
}

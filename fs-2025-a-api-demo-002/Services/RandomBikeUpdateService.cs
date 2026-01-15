// Servicio en segundo plano de la API V1. Simula cambios en las estaciones cada 15 segundos para refrescar datos.

using Microsoft.Extensions.Hosting;

namespace fs_2025_a_api_demo_002.Services
{
    // BackgroundService que llama a StationService.RandomiseStations para cumplir el requisito de datos dinámicos.
    public class RandomBikeUpdateService : BackgroundService
    {
        private readonly StationService _stationService;
        private readonly ILogger<RandomBikeUpdateService> _logger;

        // Recibe StationService e ILogger mediante inyección de dependencias.
        public RandomBikeUpdateService(StationService stationService, ILogger<RandomBikeUpdateService> logger)
        {
            _stationService = stationService;
            _logger = logger;
        }

        // Bucle principal: mientras la app no se cancele, modifica estaciones y espera 15 s.
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Cambiamos valores de puestos y bicis para simular flujo en vivo.
                    _stationService.RandomiseStations();
                    _logger.LogInformation("Stations updated with random values at {Time}", DateTimeOffset.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to randomise stations");
                }

                // Pausa de 15 segundos entre cada actualización.
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
        }
    }
}

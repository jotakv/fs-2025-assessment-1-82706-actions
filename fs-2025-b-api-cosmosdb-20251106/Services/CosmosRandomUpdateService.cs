// Servicio en segundo plano de la API V2. Llama periódicamente a CosmosStationService para simular cambios en Cosmos DB.

using Microsoft.Extensions.Hosting;

namespace fs_2025_b_api_cosmosdb_20251106.Services
{
    // BackgroundService que mantiene los datos de estaciones en movimiento en la base Cosmos.
    public class CosmosRandomUpdateService : BackgroundService
    {
        private readonly CosmosStationService _stationService;
        private readonly ILogger<CosmosRandomUpdateService> _logger;

        // Recibe dependencias via inyección para usar en el bucle.
        public CosmosRandomUpdateService(CosmosStationService stationService, ILogger<CosmosRandomUpdateService> logger)
        {
            _stationService = stationService;
            _logger = logger;
        }

        // Ejecuta un bucle infinito hasta que se cancele la aplicación.
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Alteramos estaciones en Cosmos para simular datos vivos.
                    await _stationService.RandomiseStationsAsync();
                    _logger.LogInformation("Cosmos stations randomised at {Time}", DateTimeOffset.Now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Could not randomise cosmos stations");
                }

                // Esperamos 20 segundos antes de la próxima iteración.
                await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);
            }
        }
    }
}

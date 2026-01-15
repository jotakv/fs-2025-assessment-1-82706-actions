// Endpoints HTTP de la API V1 (JSON). Exponen CRUD, filtros y resumen bajo rutas /api/v1/...

using fs_2025_a_api_demo_002.Models;
using fs_2025_a_api_demo_002.Services;

namespace fs_2025_a_api_demo_002.Endpoints
{
    // Clase estática que registra las rutas mínimas y conecta con StationService.
    public static class BikeEndPoints
    {
        // Agrupa el mapeo de todos los endpoints versionados.
        public static void AddBikeEndPoints(this WebApplication app)
        {
            app.MapGet("/api/v1/stations", GetAllStations);
            app.MapGet("/api/v1/stations/{number:int}", GetStationByNumber);
            app.MapGet("/api/v1/stations/summary", GetSummary);
            app.MapPost("/api/v1/stations", CreateStation);
            app.MapPut("/api/v1/stations/{number:int}", UpdateStation);
        }

        // Devuelve lista de estaciones usando filtros, orden y paginación.
        private static IResult GetAllStations(
            StationService stationService,
            string? status,
            int? minBikes,
            string? q,
            string? sort,
            string? dir,
            int page = 1,
            int pageSize = 10)
        {
            // Delegamos la lógica compleja al servicio.
            var result = stationService.GetStations(status, minBikes, q, sort, dir, page, pageSize);
            return Results.Ok(result);
        }

        // Recupera una estación específica por número.
        private static IResult GetStationByNumber(StationService stationService, int number)
        {
            var station = stationService.GetStationByNumber(number);
            if (station == null)
            {
                return Results.NotFound();
            }

            return Results.Ok(station);
        }

        // Endpoint de resumen con totales globales.
        private static IResult GetSummary(StationService stationService)
        {
            var summary = stationService.GetSummary();
            return Results.Ok(summary);
        }

        // Crea una nueva estación; valida que el número no esté repetido.
        private static IResult CreateStation(StationService stationService, BikeModel station)
        {
            if (!stationService.AddStation(station))
            {
                return Results.BadRequest("Station number already exists.");
            }

            return Results.Created($"/api/v1/stations/{station.number}", station);
        }

        // Actualiza una estación existente por número.
        private static IResult UpdateStation(StationService stationService, int number, BikeModel updated)
        {
            var success = stationService.UpdateStation(number, updated);
            if (!success)
            {
                return Results.NotFound();
            }

            return Results.Ok(updated);
        }
    }
}

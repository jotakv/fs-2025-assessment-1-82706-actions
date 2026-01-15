// Servicio principal de la API V1 (JSON). Gestiona la lista en memoria, filtros, ordenación, paginación y caché.

using fs_2025_a_api_demo_002.Data;
using fs_2025_a_api_demo_002.Models;
using Microsoft.Extensions.Caching.Memory;

namespace fs_2025_a_api_demo_002.Services
{
    // Clase que ofrece operaciones sobre las estaciones cargadas desde dublinbike.json.
    public class StationService
    {
        private readonly BikeData _bikeData;
        private readonly IMemoryCache _cache;
        private readonly List<string> _cacheKeys = new List<string>();
        private readonly object _lockObject = new object();

        // Recibe los datos en memoria y la caché (inyección de dependencias).
        public StationService(BikeData bikeData, IMemoryCache cache)
        {
            _bikeData = bikeData;
            _cache = cache;
        }

        // Devuelve estaciones con filtros (status, minBikes, q), ordenación y paginación. Incluye caché en memoria.
        public List<BikeModel> GetStations(string? status, int? minBikes, string? search, string? sort, string? dir, int page, int pageSize)
        {
            // Normalizamos parámetros de página y ordenación para evitar valores inválidos.
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 10 : pageSize;
            sort = string.IsNullOrWhiteSpace(sort) ? "name" : sort;
            dir = string.IsNullOrWhiteSpace(dir) ? "asc" : dir;

            // Construimos clave única para almacenar el resultado en caché.
            string cacheKey = $"stations-{status}-{minBikes}-{search}-{sort}-{dir}-{page}-{pageSize}";
            if (_cache.TryGetValue(cacheKey, out List<BikeModel>? cachedList) && cachedList != null)
            {
                // Si existe en caché, devolvemos la copia sin recalcular filtros.
                return cachedList;
            }

            // Partimos de la lista completa en memoria.
            var query = _bikeData.Bikes.ToList();

            // Filtro por estado (OPEN/CLOSED) ignorando mayúsculas.
            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(b => b.status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Filtro por mínimo de bicis disponibles.
            if (minBikes.HasValue)
            {
                query = query.Where(b => b.available_bikes >= minBikes.Value).ToList();
            }

            // Búsqueda simple en nombre o dirección (q).
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(b =>
                    b.name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    b.address.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Aplicamos ordenación configurable (por nombre, bicis disponibles u ocupación).
            query = ApplySorting(query, sort, dir);

            // Paginamos resultados con Skip/Take.
            query = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // Guardamos en caché por 5 minutos para acelerar consultas repetidas.
            var cacheEntryOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5));
            _cache.Set(cacheKey, query, cacheEntryOptions);
            _cacheKeys.Add(cacheKey);

            return query;
        }

        // Busca una estación concreta por su número; usado por GET /stations/{number}.
        public BikeModel? GetStationByNumber(int number)
        {
            return _bikeData.Bikes.FirstOrDefault(b => b.number == number);
        }

        // Calcula el resumen global requerido (totales y estaciones abiertas/cerradas).
        public StationSummary GetSummary()
        {
            var totalStations = _bikeData.Bikes.Count;
            var totalBikeStands = _bikeData.Bikes.Sum(b => b.bike_stands);
            var totalAvailableBikes = _bikeData.Bikes.Sum(b => b.available_bikes);
            var openCount = _bikeData.Bikes.Count(b => b.status.Equals("OPEN", StringComparison.OrdinalIgnoreCase));
            var closedCount = _bikeData.Bikes.Count - openCount;

            return new StationSummary
            {
                TotalStations = totalStations,
                TotalBikeStands = totalBikeStands,
                TotalAvailableBikes = totalAvailableBikes,
                OpenStations = openCount,
                ClosedStations = closedCount
            };
        }

        // Añade una nueva estación (POST) asegurando que el número no se repita.
        public bool AddStation(BikeModel station)
        {
            lock (_lockObject)
            {
                if (_bikeData.Bikes.Any(b => b.number == station.number))
                {
                    return false;
                }

                // Preparamos campos calculados antes de guardar.
                station.id = station.number.ToString();
                station.last_update = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                station.available_bike_stands = Math.Max(0, station.bike_stands - station.available_bikes);
                _bikeData.Bikes.Add(station);
                ClearCache();
                return true;
            }
        }

        // Actualiza una estación existente (PUT) y recalcula valores derivados.
        public bool UpdateStation(int number, BikeModel updated)
        {
            lock (_lockObject)
            {
                var station = _bikeData.Bikes.FirstOrDefault(b => b.number == number);
                if (station == null)
                {
                    return false;
                }

                station.name = updated.name;
                station.address = updated.address;
                station.status = updated.status;
                station.bike_stands = updated.bike_stands;
                // Nunca dejamos available_bikes mayor que bike_stands.
                station.available_bikes = Math.Min(updated.available_bikes, updated.bike_stands);
                station.available_bike_stands = Math.Max(0, station.bike_stands - station.available_bikes);
                station.position = updated.position;
                station.last_update = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                ClearCache();
                return true;
            }
        }

        // Usado por el BackgroundService para simular cambios cada cierto tiempo.
        public void RandomiseStations()
        {
            lock (_lockObject)
            {
                var random = new Random();
                foreach (var station in _bikeData.Bikes)
                {
                    // Generamos nuevos números de puestos y bicis disponibles manteniendo coherencia.
                    var newStands = random.Next(5, 40);
                    var newBikes = random.Next(0, newStands + 1);
                    station.bike_stands = newStands;
                    station.available_bikes = newBikes;
                    station.available_bike_stands = newStands - newBikes;
                    station.last_update = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                }
                ClearCache();
            }
        }

        // Ordena la lista según parámetro sort y dirección asc/desc.
        private List<BikeModel> ApplySorting(List<BikeModel> query, string sort, string dir)
        {
            bool descending = dir.Equals("desc", StringComparison.OrdinalIgnoreCase);
            switch (sort.ToLower())
            {
                case "availablebikes":
                    query = descending
                        ? query.OrderByDescending(b => b.available_bikes).ToList()
                        : query.OrderBy(b => b.available_bikes).ToList();
                    break;
                case "occupancy":
                    query = descending
                        ? query.OrderByDescending(b => b.Occupancy).ToList()
                        : query.OrderBy(b => b.Occupancy).ToList();
                    break;
                default:
                    query = descending
                        ? query.OrderByDescending(b => b.name).ToList()
                        : query.OrderBy(b => b.name).ToList();
                    break;
            }

            return query;
        }

        // Limpia todas las entradas de caché relacionadas cuando los datos cambian.
        private void ClearCache()
        {
            foreach (var key in _cacheKeys)
            {
                _cache.Remove(key);
            }
            _cacheKeys.Clear();
        }
    }

    // Objeto de resumen con totales básicos solicitado en el enunciado.
    public class StationSummary
    {
        public int TotalStations { get; set; }
        public int TotalBikeStands { get; set; }
        public int TotalAvailableBikes { get; set; }
        public int OpenStations { get; set; }
        public int ClosedStations { get; set; }
    }
}

// Servicio de estaciones para la API V2. Consulta, crea y actualiza estaciones en Azure Cosmos DB con filtros y caché.

using fs_2025_a_api_demo_002.Models;
using fs_2025_a_api_demo_002.Services;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Serialization.HybridRow.Schemas;
using Microsoft.Extensions.Caching.Memory;
using PartitionKey = Microsoft.Azure.Cosmos.PartitionKey;

namespace fs_2025_b_api_cosmosdb_20251106.Services
{
    // Maneja toda la lógica de acceso a Cosmos DB para las estaciones.
    public class CosmosStationService
    {
        private readonly Container _container;
        private readonly IMemoryCache _cache;
        private readonly List<string> _cacheKeys = new List<string>();

        // Recibe el contenedor de Cosmos y la caché en memoria (DI).
        public CosmosStationService(Container container, IMemoryCache cache)
        {
            _container = container;
            _cache = cache;
        }

        // Devuelve estaciones desde Cosmos aplicando filtros, ordenación y paginación. Guarda resultado en caché.
        public async Task<List<BikeModel>> GetStationsAsync(string? status, int? minBikes, string? search, string? sort, string? dir, int page, int pageSize)
        {
            // Normalizamos parámetros para evitar valores cero o nulos.
            page = page <= 0 ? 1 : page;
            pageSize = pageSize <= 0 ? 10 : pageSize;
            sort = string.IsNullOrWhiteSpace(sort) ? "name" : sort;
            dir = string.IsNullOrWhiteSpace(dir) ? "asc" : dir;

            // Revisamos si la combinación ya está en caché.
            string cacheKey = $"v2-{status}-{minBikes}-{search}-{sort}-{dir}-{page}-{pageSize}";
            if (_cache.TryGetValue(cacheKey, out List<BikeModel>? cached) && cached != null)
            {
                return cached;
            }

            // Cargamos todas las estaciones desde Cosmos en memoria para filtrar con LINQ.
            var allStations = await LoadAllStationsAsync();

            // Filtro por estado.
            if (!string.IsNullOrWhiteSpace(status))
            {
                allStations = allStations.Where(s => s.status.Equals(status, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Filtro por mínimo de bicis.
            if (minBikes.HasValue)
            {
                allStations = allStations.Where(s => s.available_bikes >= minBikes.Value).ToList();
            }

            // Búsqueda en nombre o dirección.
            if (!string.IsNullOrWhiteSpace(search))
            {
                allStations = allStations.Where(s => s.name.Contains(search, StringComparison.OrdinalIgnoreCase)
                    || s.address.Contains(search, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            // Ordenamos y aplicamos paginación igual que en V1.
            allStations = ApplySorting(allStations, sort, dir);
            allStations = allStations.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            // Guardamos copia en caché 5 minutos para acelerar llamadas repetidas.
            _cache.Set(cacheKey, allStations, new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5)));
            _cacheKeys.Add(cacheKey);
            return allStations;
        }

        // Busca una estación por número usando una consulta SQL a Cosmos.
        public async Task<BikeModel?> GetStationAsync(int number)
        {
            // Buscamos por el campo number, no por id
            var query = new QueryDefinition(
                "SELECT TOP 1 * FROM c WHERE c.number = @number")
                .WithParameter("@number", number);

            using var iterator = _container.GetItemQueryIterator<BikeModel>(query);

            if (!iterator.HasMoreResults)
            {
                return null;
            }

            var response = await iterator.ReadNextAsync();
            var station = response.Resource.FirstOrDefault();

            return station; // puede ser null si no hay resultados
        }


        // Calcula totales de estaciones directamente desde la colección en Cosmos.
        public async Task<StationSummary> GetSummaryAsync()
        {
            var all = await LoadAllStationsAsync();
            return new StationSummary
            {
                TotalStations = all.Count,
                TotalBikeStands = all.Sum(s => s.bike_stands),
                TotalAvailableBikes = all.Sum(s => s.available_bikes),
                OpenStations = all.Count(s => s.status.Equals("OPEN", StringComparison.OrdinalIgnoreCase)),
                ClosedStations = all.Count(s => s.status.Equals("CLOSED", StringComparison.OrdinalIgnoreCase))
            };
        }

        // Crea una nueva estación en Cosmos. Usa Guid para id para no chocar con otras particiones.
        public async Task<bool> CreateStationAsync(BikeModel station)
        {
            station.id = Guid.NewGuid().ToString();
            try
            {
                // CreateItemAsync inserta un nuevo documento en Cosmos con partition key /id.
                await _container.CreateItemAsync(station, new PartitionKey(station.id));
                ClearCache();
                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
            {
                return false;
            }
        }

        // Reemplaza una estación existente identificada por number (no por id).
        public async Task<bool> UpdateStationAsync(int number, BikeModel updated)
        {
            try
            {
                // 1) Buscar el documento que tenga ese number
                var query = new QueryDefinition(
                    "SELECT TOP 1 c.id FROM c WHERE c.number = @number")
                    .WithParameter("@number", number);

                using var iterator = _container.GetItemQueryIterator<BikeModel>(query);

                if (!iterator.HasMoreResults)
                {
                    // No hay ninguna estacin con ese number
                    return false;
                }

                var response = await iterator.ReadNextAsync();
                var existing = response.Resource.FirstOrDefault();

                if (existing == null)
                {
                    // Tampoco encontramos nada
                    return false;
                }

                // 2) Forzar coherencia: el modelo actualizado debe usar ese id y ese number
                updated.id = existing.id;
                updated.number = number;

                // 3) Reemplazar el documento existente usando su id (partition key = /id)
                await _container.ReplaceItemAsync(
                    updated,
                    updated.id,
                    new PartitionKey(updated.id));

                ClearCache();
                return true;
            }
            catch (CosmosException)
            {
                return false;
            }
        }


        // Cambia aleatoriamente valores de las estaciones para simular datos en vivo.
        public async Task RandomiseStationsAsync()
        {

            var stations = await LoadAllStationsAsync();
            var random = new Random();
            foreach (var station in stations)
            {
                // Generamos nuevos puestos y bicis disponibles, asegurando coherencia de conteos.
                var newStands = random.Next(5, 40);
                var newBikes = random.Next(0, newStands + 1);
                station.bike_stands = newStands;
                station.available_bikes = newBikes;
                station.available_bike_stands = newStands - newBikes;
                station.last_update = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                //station.id = station.id.ToString();

                // UpsertItemAsync actualiza si existe o inserta si falta, usando la clave de partición.
                await _container.UpsertItemAsync(station, new PartitionKey(station.id));
            }
            ClearCache();
        }

        // Consulta todas las estaciones en Cosmos para operar con ellas en memoria.
        private async Task<List<BikeModel>> LoadAllStationsAsync()
        {
            var query = new QueryDefinition("SELECT * FROM c");
            var iterator = _container.GetItemQueryIterator<BikeModel>(query);
            var stations = new List<BikeModel>();
            while (iterator.HasMoreResults)
            {
                // Leemos página por página hasta obtener toda la colección.
                var response = await iterator.ReadNextAsync();
                stations.AddRange(response.Resource);
            }
            return stations;
        }

        // Ordena resultados igual que en la versión JSON.
        private List<BikeModel> ApplySorting(List<BikeModel> items, string sort, string dir)
        {
            bool descending = dir.Equals("desc", StringComparison.OrdinalIgnoreCase);
            switch (sort.ToLower())
            {
                case "availablebikes":
                    items = descending ? items.OrderByDescending(s => s.available_bikes).ToList() : items.OrderBy(s => s.available_bikes).ToList();
                    break;
                case "occupancy":
                    items = descending ? items.OrderByDescending(s => s.Occupancy).ToList() : items.OrderBy(s => s.Occupancy).ToList();
                    break;
                default:
                    items = descending ? items.OrderByDescending(s => s.name).ToList() : items.OrderBy(s => s.name).ToList();
                    break;
            }

            return items;
        }

        // Elimina las entradas de caché almacenadas cuando cambia algún dato.
        private void ClearCache()
        {
            foreach (var key in _cacheKeys)
            {
                _cache.Remove(key);
            }
            _cacheKeys.Clear();
        }

    }
}

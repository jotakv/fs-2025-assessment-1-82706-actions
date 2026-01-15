// Datos en memoria para la API V1 (JSON). Carga el archivo dublinbike.json al iniciar y guarda la lista de estaciones.

using fs_2025_a_api_demo_002.Models;
using System.Text.Json;

namespace fs_2025_a_api_demo_002.Data
{
    // Clase que mantiene la colección de estaciones en memoria para que los servicios la usen.
    public class BikeData
    {
        public List<BikeModel> Bikes { get; private set; } = new List<BikeModel>();

        // Constructor vacío usado por DI; delega en el otro constructor.
        public BikeData() : this(null)
        {
        }

        // Constructor que admite datos de prueba (tests) o carga el JSON real al iniciar la API.
        public BikeData(List<BikeModel>? seedData)
        {
            // Si vienen datos semilla desde pruebas, los usamos y evitamos leer archivo.
            if (seedData != null)
            {
                Bikes = seedData;
                return;
            }

            // Opciones para leer el JSON sin importar mayúsculas/minúsculas en nombres de propiedades.
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            // Ruta del archivo dublinbike.json incluido en el proyecto.
            string filePath = Path.Combine(AppContext.BaseDirectory, "Data", "dublinbike.json");
            if (!File.Exists(filePath))
            {
                // Si no existe el archivo, devolvemos lista vacía para evitar fallos.
                Bikes = new List<BikeModel>();
                return;
            }

            // Leemos y convertimos el JSON en objetos BikeModel almacenados en memoria.
            var jsonData = File.ReadAllText(filePath);
            Bikes = JsonSerializer.Deserialize<List<BikeModel>>(jsonData, options) ?? new List<BikeModel>();
            foreach (var bike in Bikes)
            {
                // En V1 usamos id igual al number para parecerse a Cosmos DB.
                bike.id = bike.number.ToString();
            }
        }

        // Método auxiliar usado por el background service para reemplazar toda la lista.
        public void ReplaceAll(List<BikeModel> bikes)
        {
            Bikes = bikes;
        }
    }
}

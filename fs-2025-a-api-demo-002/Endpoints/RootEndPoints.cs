// Endpoints raíz para la API V1. Solo muestra un mensaje de estado para comprobar que el servicio está activo.

namespace fs_2025_a_api_demo_002.Endpoints
{
    // Clase estática con método de extensión para registrar la ruta "/".
    public static class RootEndPoints
    {
        // Mapea GET / que devuelve un mensaje simple; útil para pruebas rápidas.
        public static void AddRootEndPoints(this WebApplication app)
        {
            app.MapGet("/", () => "Dublin Bikes API - V1 (JSON) running");
        }
    }
}

// Archivo principal de la API V2 (Cosmos DB). Configura el cliente Cosmos y registra los endpoints /api/v2/...

using fs_2025_a_api_demo_002.Models;
using fs_2025_b_api_cosmosdb_20251106.Services;
using Microsoft.Azure.Cosmos;

var builder = WebApplication.CreateBuilder(args);

// Servicios generales de documentación y caché.
builder.Services.AddMemoryCache();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var configuration = builder.Configuration;
var cosmosDbEndpoint = configuration["CosmosDb:EndpointUri"] ?? "";
var cosmosDbKey = configuration["CosmosDb:PrimaryKey"] ?? "";
var databaseName = configuration["CosmosDb:DatabaseName"] ?? "bikes";
var containerName = configuration["CosmosDb:ContainerName"] ?? "stations";

if (string.IsNullOrWhiteSpace(cosmosDbEndpoint) || string.IsNullOrWhiteSpace(cosmosDbKey))
{
    throw new InvalidOperationException("Cosmos DB settings are missing. Add CosmosDb:EndpointUri and CosmosDb:PrimaryKey.");
}

// Creamos el cliente Cosmos y garantizamos que la base de datos y el contenedor existan.
var client = new CosmosClient(cosmosDbEndpoint, cosmosDbKey);
var database = await client.CreateDatabaseIfNotExistsAsync(databaseName);
var container = await database.Database.CreateContainerIfNotExistsAsync(new ContainerProperties
{
    Id = containerName,
    PartitionKeyPath = "/id"
});

// Registramos contenedor Cosmos, servicio de estaciones y background service aleatorio.
builder.Services.AddSingleton(container.Container);
builder.Services.AddSingleton<CosmosStationService>();
builder.Services.AddHostedService<CosmosRandomUpdateService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Endpoint raíz simple para saber que la versión Cosmos está funcionando.
app.MapGet("/", () => "Dublin Bikes API - V2 (Cosmos) running");
// Endpoints con filtros, paginación y CRUD iguales a la versión V1 pero usando CosmosStationService.
app.MapGet("/api/v2/stations", async (CosmosStationService service, string? status, int? minBikes, string? q, string? sort, string? dir, int page, int pageSize)
    => Results.Ok(await service.GetStationsAsync(status, minBikes, q, sort, dir, page, pageSize)));
app.MapGet("/api/v2/stations/{number:int}", async (CosmosStationService service, int number) =>
{
    var station = await service.GetStationAsync(number);
    return station == null ? Results.NotFound() : Results.Ok(station);
});
app.MapGet("/api/v2/stations/summary", async (CosmosStationService service) => Results.Ok(await service.GetSummaryAsync()));
app.MapPost("/api/v2/stations", async (CosmosStationService service, BikeModel station) =>
{
    var created = await service.CreateStationAsync(station);
    return created ? Results.Created($"/api/v2/stations/{station.number}", station) : Results.BadRequest("Station number already exists.");
});
app.MapPut("/api/v2/stations/{number:int}", async (CosmosStationService service, int number, BikeModel updated) =>
{
    var updatedOk = await service.UpdateStationAsync(number, updated);
    return updatedOk ? Results.Ok(updated) : Results.NotFound();
});

app.Run();

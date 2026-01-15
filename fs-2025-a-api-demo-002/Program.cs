// Archivo principal de la API V1 (JSON). Configura el host minimal API y registra los endpoints de Dublin Bikes.

using fs_2025_a_api_demo_002.Endpoints;
using fs_2025_a_api_demo_002.Startup;

var builder = WebApplication.CreateBuilder(args);

// Aquí se agregan servicios básicos de Swagger para que la API sea fácil de explorar.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Registramos las dependencias propias del ejercicio (servicios, datos en memoria y background service).
builder.AddDependencies();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
// Endpoint raíz solo para avisar que la API V1 está en marcha.
app.AddRootEndPoints();
// Endpoints CRUD versionados /api/v1/... que usan StationService.
app.AddBikeEndPoints();

app.Run();

// Clase parcial necesaria para las pruebas de WebApplicationFactory.
public partial class Program { }

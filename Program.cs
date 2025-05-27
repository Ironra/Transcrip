// Importa el servicio IVoskService para inyectarlo en el contenedor.
using VoskRealtimeApi.Services;

// Importa clases necesarias para generar documentación Swagger/OpenAPI.
using Microsoft.OpenApi.Models;           
using Swashbuckle.AspNetCore.SwaggerGen;  // Para configurar Swagger
using Swashbuckle.AspNetCore.SwaggerUI;   // Para la interfaz Swagger UI

// Crea el constructor del host de la aplicación ASP.NET Core.
var builder = WebApplication.CreateBuilder(args);

// Intenta obtener el puerto desde la variable de entorno PORT, o usa 5142 por defecto.
var port = Environment.GetEnvironmentVariable("PORT") ?? "5142";

// Configura el host para escuchar en cualquier IP (0.0.0.0) y en el puerto especificado.
builder.WebHost.UseUrls($"http://*:{port}");

// Registra el servicio de Vosk como singleton (una única instancia compartida).
builder.Services.AddSingleton<IVoskService, VoskService>();

// Agrega soporte para controladores (controladores MVC/API).
builder.Services.AddControllers();

// Agrega soporte para la exploración de endpoints (útil para Swagger en minimal APIs).
builder.Services.AddEndpointsApiExplorer();

// Configura Swagger y añade documentación para la API.
builder.Services.AddSwaggerGen(options =>
{
    // Define el documento OpenAPI con título y versión.
    options.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "VoskRealtimeApi", 
        Version = "v1" 
    });
});
// Construye la aplicación a partir del builder.
var app = builder.Build();

// Si el entorno es de desarrollo, habilita Swagger y su interfaz gráfica.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();        // Habilita la generación del documento Swagger JSON.
    app.UseSwaggerUI();      // Habilita la interfaz gráfica Swagger UI.
}
// Habilita el middleware de WebSockets.
app.UseWebSockets();

// Sirve archivos estáticos desde la carpeta wwwroot (por ejemplo, JS, HTML, CSS).
app.UseStaticFiles();

// Sirve por defecto index.html si se accede a la raíz sin especificar archivo.
app.UseDefaultFiles();            

// Mapea los controladores a sus rutas (por ejemplo: /ws).
app.MapControllers();

// Si no encuentra ninguna ruta, responde con el archivo index.html (SPA fallback).
app.MapFallbackToFile("index.html");

// Inicia la aplicación.
app.Run();

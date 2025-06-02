using VoskRealtimeApi.Services;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Intenta obtener el puerto desde la variable de entorno PORT, o usa 5142 por defecto.
var port = Environment.GetEnvironmentVariable("PORT") ?? "5142";

// Configura el host para escuchar en cualquier IP (0.0.0.0) y en el puerto especificado.
builder.WebHost.UseUrls($"http://*:{port}");

builder.Services.AddSingleton<IVoskService, VoskService>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();


builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "VoskRealtimeApi", 
        Version = "v1" 
    });
});
var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();   
    app.UseSwaggerUI();      
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

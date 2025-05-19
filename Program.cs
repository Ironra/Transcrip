using VoskRealtimeApi.Services;
using Microsoft.OpenApi.Models;           // Para OpenApiInfo
using Swashbuckle.AspNetCore.SwaggerGen;  // Para AddSwaggerGen()
using Swashbuckle.AspNetCore.SwaggerUI;
var builder = WebApplication.CreateBuilder(args);
// Registrar el servicio de Vosk
builder.Services.AddSingleton<IVoskService, VoskService>();

// Añadir controladores
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();  // sólo para minimal APIs
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo 
    { 
        Title = "VoskRealtimeApi", 
        Version = "v1" 
    });
});
// Habilitar WebSockets
// builder.Services.AddWebSockets(options =>
// {
//     // Ajustes opcionales aquí
// });

// OpenAPI/Swagger (opcional)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Swagger UI en desarrollo
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//app.UseHttpsRedirection();

// Middleware de WebSockets
app.UseWebSockets();

app.UseStaticFiles();             // Habilita wwwroot
app.UseDefaultFiles();            // Busca index.html por defecto
app.UseWebSockets();              
app.MapControllers();
// Mapear controladores (incluye /ws)
app.MapControllers();

app.Run();

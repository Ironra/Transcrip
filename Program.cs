
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Net.WebSockets;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Habilitar WebSockets
var webSocketOptions = new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(120)
};
app.UseWebSockets(webSocketOptions);

// Middleware WebSocket
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/ws" && context.WebSockets.IsWebSocketRequest)
    {
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        Console.WriteLine("WebSocket conectado");

        var buffer = new byte[1024 * 4];
        WebSocketReceiveResult result;
        do
        {
            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
            Console.WriteLine($"Mensaje recibido: {message}");

            var response = Encoding.UTF8.GetBytes("Echo: " + message);
            await webSocket.SendAsync(new ArraySegment<byte>(response), result.MessageType, result.EndOfMessage, CancellationToken.None);

        } while (!result.CloseStatus.HasValue);

        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        Console.WriteLine("WebSocket desconectado");
    }
    else
    {
        await next();
    }
});

// Servir archivos estáticos desde wwwroot (incluye index.html)
app.UseDefaultFiles(); // Sirve index.html automáticamente
app.UseStaticFiles();

app.Run();



















// using VoskRealtimeApi.Services;
// using Microsoft.OpenApi.Models;           // Para OpenApiInfo
// using Swashbuckle.AspNetCore.SwaggerGen;  // Para AddSwaggerGen()
// using Swashbuckle.AspNetCore.SwaggerUI;
// var builder = WebApplication.CreateBuilder(args);


// var port = Environment.GetEnvironmentVariable("PORT") ?? "5142";
// builder.WebHost.UseUrls($"http://0.0.0.0:{port}");
// // Registrar el servicio de Vosk
// builder.Services.AddSingleton<IVoskService, VoskService>();

// // Añadir controladores
// builder.Services.AddControllers();
// builder.Services.AddEndpointsApiExplorer();  // sólo para minimal APIs
// builder.Services.AddSwaggerGen(options =>
// {
//     options.SwaggerDoc("v1", new OpenApiInfo 
//     { 
//         Title = "VoskRealtimeApi", 
//         Version = "v1" 
//     });
// });
// // Habilitar WebSockets
// // builder.Services.AddWebSockets(options =>
// // {
// //     // Ajustes opcionales aquí
// // });

// // OpenAPI/Swagger (opcional)
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();

// var app = builder.Build();

// // Swagger UI en desarrollo
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

// //app.UseHttpsRedirection();

// // Middleware de WebSockets
// app.UseWebSockets();

// app.UseStaticFiles();             // Habilita wwwroot
// app.UseDefaultFiles();            // Busca index.html por defecto
// app.UseWebSockets();              
// app.MapControllers();
// // Mapear controladores (incluye /ws)
// app.MapControllers();
// app.MapFallbackToFile("index.html");
// app.Run();

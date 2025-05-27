// Importa el espacio de nombres para trabajar con WebSockets.
using System.Net.WebSockets;

// Importa utilidades para trabajar con codificación de texto.
using System.Text;

// Importa componentes necesarios para controladores en ASP.NET Core.
using Microsoft.AspNetCore.Mvc;

// Importa el servicio que maneja la lógica de reconocimiento de voz.
using VoskRealtimeApi.Services;

namespace VoskRealtimeApi.Controllers
{
    // Marca esta clase como un controlador API.
    [ApiController]

    // Define la ruta base del controlador como "/ws" (WebSocket endpoint).
    [Route("/ws")]
    public class SpeechWebSocketController : ControllerBase
    {
        // Servicio inyectado para manejar Vosk (procesamiento de audio).
        private readonly IVoskService _vosk;

        // Constructor que recibe el servicio Vosk por inyección de dependencias.
        public SpeechWebSocketController(IVoskService vosk) => _vosk = vosk;

        // Acción HTTP GET que maneja conexiones WebSocket.
        [HttpGet]
        public async Task Get()
        {
            // Si la solicitud no es de tipo WebSocket, retorna un error 400.
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                HttpContext.Response.StatusCode = 400;
                return;
            }
            // Acepta la conexión WebSocket.
            using var ws = await HttpContext.WebSockets.AcceptWebSocketAsync();

            // Buffer para almacenar los datos recibidos (tamaño 8192 bytes).
            var buffer = new byte[8192];

            // Variable para almacenar los resultados de recepción.
            WebSocketReceiveResult result;

            // Bucle principal que se ejecuta mientras el WebSocket esté abierto.
            while (ws.State == WebSocketState.Open)
            {
                // Espera y recibe datos del cliente WebSocket.
                result = await ws.ReceiveAsync(buffer, CancellationToken.None);

                // Si el cliente cierra la conexión, se termina el bucle.
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Console.WriteLine("[WS] Cliente cerró la conexión.");
                    break;
                }
                // Si se recibieron datos, los procesa con Vosk.
                if (result.Count > 0)
                {
                    Console.WriteLine($"[WS] Recibidos {result.Count} bytes, tipo: {result.MessageType}");

                    // Envía los datos a Vosk y obtiene una respuesta en JSON.
                    var json = await _vosk.AcceptWaveformAsync(buffer, result.Count);

                    // Codifica la respuesta JSON a bytes UTF-8.
                    var msg = Encoding.UTF8.GetBytes(json);

                    // Envía la respuesta al cliente WebSocket.
                    await ws.SendAsync(msg, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
            // Al terminar el bucle, imprime el estado actual del WebSocket.
            Console.WriteLine("[WS] Bucle terminado. Estado: " + ws.State);

            // Genera y envía el resultado final desde Vosk al cerrar la conexión.
            var finalJson = _vosk.FinalResult();
            await ws.SendAsync(Encoding.UTF8.GetBytes(finalJson),
                               WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}

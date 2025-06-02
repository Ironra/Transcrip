using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using VoskRealtimeApi.Services;

namespace VoskRealtimeApi.Controllers
{
    [ApiController]

    // Define la ruta base del controlador como "/ws" (WebSocket endpoint).
    [Route("/ws")]
    public class SpeechWebSocketController : ControllerBase
    {
        private readonly IVoskService _vosk;
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
            var buffer = new byte[8192];
            WebSocketReceiveResult result;
            while (ws.State == WebSocketState.Open)
            {
                result = await ws.ReceiveAsync(buffer, CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    Console.WriteLine("[WS] Cliente cerró la conexión.");
                    break;
                }
                if (result.Count > 0)
                {
                    Console.WriteLine($"[WS] Recibidos {result.Count} bytes, tipo: {result.MessageType}");
                    // Envía los datos a Vosk y obtiene una respuesta en JSON.
                    var json = await _vosk.AcceptWaveformAsync(buffer, result.Count);
                    // Codifica la respuesta JSON a bytes UTF-8.
                    var msg = Encoding.UTF8.GetBytes(json);
                    await ws.SendAsync(msg, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
            Console.WriteLine("[WS] Bucle terminado. Estado: " + ws.State);
            // Genera y envía el resultado final desde Vosk al cerrar la conexión.
            var finalJson = _vosk.FinalResult();
            await ws.SendAsync(Encoding.UTF8.GetBytes(finalJson),
                               WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}

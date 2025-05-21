using System.Net.WebSockets;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using VoskRealtimeApi.Services;

namespace VoskRealtimeApi.Controllers
{
    [ApiController]
    [Route("/ws")]
    public class SpeechWebSocketController : ControllerBase
    {
        private readonly IVoskService _vosk;

        public SpeechWebSocketController(IVoskService vosk) => _vosk = vosk;

        [HttpGet]
        public async Task Get()
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                HttpContext.Response.StatusCode = 400;
                return;
            }

            using var ws = await HttpContext.WebSockets.AcceptWebSocketAsync();
            var buffer = new byte[8192];
            WebSocketReceiveResult result;


            //    while ((result = await ws.ReceiveAsync(buffer, CancellationToken.None)).Count > 0)
            //     {
            //         Console.WriteLine($"[WS] Recibidos {result.Count} bytes, tipo: {result.MessageType}");
            //         var json = await _vosk.AcceptWaveformAsync(buffer, result.Count);
            //         var msg = Encoding.UTF8.GetBytes(json);
            //         await ws.SendAsync(msg, WebSocketMessageType.Text, true, CancellationToken.None);
            //     }
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
                    var json = await _vosk.AcceptWaveformAsync(buffer, result.Count);
                    var msg = Encoding.UTF8.GetBytes(json);
                    await ws.SendAsync(msg, WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
Console.WriteLine("[WS] Bucle terminado. Estado: " + ws.State);
            // Envía el resultado final al cerrar
            var finalJson = _vosk.FinalResult();
            await ws.SendAsync(Encoding.UTF8.GetBytes(finalJson),
                              WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}

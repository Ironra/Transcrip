// Services/VoskService.cs
using System.IO;
using Vosk;

namespace VoskRealtimeApi.Services
{
    public class VoskService : IVoskService
    {
        private readonly Model _model;
        private readonly VoskRecognizer _recognizer;

        public VoskService()
        {
            _model = new Model("Models/vosk-model-es-0.42");
            _recognizer = new VoskRecognizer(_model, 16000.0f);
        }

        public Task<string> AcceptWaveformAsync(byte[] buffer, int length)
        {
            // Procesa un bloque de audio
            if (_recognizer.AcceptWaveform(buffer, length))
                return Task.FromResult(_recognizer.Result());
            return Task.FromResult(_recognizer.PartialResult());
        }

        public string FinalResult() => _recognizer.FinalResult();
    }
}

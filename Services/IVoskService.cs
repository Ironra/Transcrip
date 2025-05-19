using System.IO;
using System.Threading.Tasks;

namespace VoskRealtimeApi.Services
{
    public interface IVoskService
    {
        Task<string> AcceptWaveformAsync(byte[] buffer, int length);
        string FinalResult();
    }
}
using System.Threading.Tasks;

namespace Coflnet.Sky.Commands.MC
{
    public abstract class McCommand
    {
        public string COFLNET => MinecraftSocket.COFLNET;
        public string DEFAULT_COLOR => McColorCodes.GRAY;
        public abstract Task Execute(MinecraftSocket socket, string arguments);
    }
}
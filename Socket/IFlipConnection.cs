using Coflnet.Sky.Filter;
using hypixel;

namespace Coflnet.Sky.Commands
{
    public interface IFlipConnection
    {
        /// <summary>
        /// Tries to send a flip, returns false if the connection can no longer send flips
        /// </summary>
        /// <param name="flip"></param>
        /// <returns></returns>
        bool SendFlip(hypixel.FlipInstance flip);
        bool SendFlip(LowPricedAuction flip);
        bool SendSold(string uuid);
        FlipSettings Settings { get; }
        long Id { get; }

        void UpdateSettings(SettingsChange settings);
    }
}
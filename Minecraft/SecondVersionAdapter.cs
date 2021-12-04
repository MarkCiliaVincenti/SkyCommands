using System;
using System.Linq;
using hypixel;

namespace Coflnet.Sky.Commands.MC
{
    public class SecondVersionAdapter : IModVersionAdapter
    {
        MinecraftSocket socket;

        public SecondVersionAdapter(MinecraftSocket socket)
        {
            this.socket = socket;
        }

        public bool SendFlip(FlipInstance flip)
        {
            var message = socket.GetFlipMsg(flip);
            var openCommand = "/viewauction " + flip.Uuid;
            var extraText = "\n" + String.Join(McColorCodes.DARK_GRAY + ", " + McColorCodes.WHITE, flip.Interesting.Take(socket.Settings.Visibility?.ExtraInfoMax ?? 0));
            
            SendMessage(new ChatPart(message, openCommand, string.Join('\n', flip.Interesting.Select(s => "・" + s)) + "\n" + flip.SellerName),
                new ChatPart(" [?]", "/cofl reference " + flip.Uuid, "Get reference auctions"),
                new ChatPart(" ❤", $"/cofl rate {flip.Uuid} {flip.Finder} up", "Vote this flip up"),
                new ChatPart("✖ ", $"/cofl rate {flip.Uuid} {flip.Finder} down", "Vote this flip down"),
                new ChatPart(extraText, openCommand, null));
 
            if (socket.Settings.ModSettings?.PlaySoundOnFlip ?? false && flip.Profit > 1_000_000)
                SendSound("note.pling", (float)(1 / (Math.Sqrt((float)flip.Profit / 1_000_000) + 1)));
            return true;
        }

        public void SendMessage(params ChatPart[] parts)
        {
            socket.Send(Response.Create("chatMessage", parts));
        }

        public void SendSound(string name, float pitch = 1)
        {
            socket.Send(Response.Create("playSound", new { name, pitch }));
        }
    }
}
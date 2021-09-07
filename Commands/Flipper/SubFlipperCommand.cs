using System;
using System.Threading.Tasks;
using Coflnet.Sky.Filter;

namespace hypixel
{
    public class SubFlipperCommand : Command
    {
        public override async Task Execute(MessageData data)
        {
            var con = (data as SocketMessageData).Connection;
            try
            {
                con.SubFlipMsgId = (int)data.mId;
                try
                {
                    con.Settings = data.GetAs<FlipSettings>();
                    if(con.Settings == null)
                        con.Settings = new FlipSettings();
                } catch(Exception)
                {
                    // could not get it continue with default
                    con.Settings = new FlipSettings();
                }
                Console.WriteLine(JSON.Stringify(con.Settings));

                var lastSettings = con.LastSettingsChange;
                lastSettings.Settings = con.Settings;
                lastSettings.UserId = data.UserId;
                await FlipperService.Instance.UpdateSettings(lastSettings);
                if (!data.User.HasPremium)
                    FlipperService.Instance.AddNonConnection(con);
                else
                {
                    Console.WriteLine("new premium con");
                    FlipperService.Instance.AddConnection(con);
                    FlipperService.Instance.RemoveNonConnection(con);
                }
            }
            catch (CoflnetException)
            {
                FlipperService.Instance.AddNonConnection(con);
            }
            await  data.Ok();
        }
    }
}
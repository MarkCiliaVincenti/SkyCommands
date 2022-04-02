using System;
using System.Linq;
using System.Threading.Tasks;
using Coflnet.Sky.Commands.Shared;
using Coflnet.Sky.Filter;
using Microsoft.Extensions.DependencyInjection;
using Coflnet.Sky.Core;

namespace Coflnet.Sky.Commands
{
    public class SubFlipperCommand : Command
    {
        public override async Task Execute(MessageData data)
        {
            var con = (data as SocketMessageData).Connection;
            var settings = GetSettings(data);
            try
            {
                con.OldFallbackSettings = settings;
                con.SubFlipMsgId = (int)data.mId;
                var userId = data.UserId;
                var updateTask = UpdateSettings(data, settings, userId);

                var lastSettings = con.LatestSettings;


                if (!data.User.HasPremium)
                    FlipperService.Instance.AddNonConnection(con);
                else
                {
                    FlipperService.Instance.AddConnection(con);

                    lastSettings.Tier = AccountTier.PREMIUM;
                    lastSettings.ExpiresAt = data.User.PremiumExpires;
                }


                lastSettings.Settings = con.Settings;
                lastSettings.UserId = userId;
                if (lastSettings.Settings.AllowedFinders == LowPricedAuction.FinderType.UNKOWN)
                    lastSettings.Settings.AllowedFinders = LowPricedAuction.FinderType.FLIPPER;

                await UpdateAccountInfo(data, lastSettings);
                await updateTask;
                await data.Ok();

                if (MessagePack.MessagePackSerializer.Serialize(con.Settings).SequenceEqual(MessagePack.MessagePackSerializer.Serialize(lastSettings.Settings)))
                    return; // nothing actually changed

                await FlipperService.Instance.UpdateSettings(lastSettings);
                return;
            }
            catch (CoflnetException e)
            {

                FlipperService.Instance.AddNonConnection(con);
            }
            await data.Ok();
        }

        public static async Task UpdateAccountInfo(MessageData data, SettingsChange lastSettings)
        {
            var service = DiHandler.ServiceProvider.GetRequiredService<SettingsService>();
            var con = (data as SocketMessageData).Connection;
            AccountInfo accountInfo = SettingsToAccountInfo(lastSettings);
            try
            {
                await service.UpdateSetting(data.UserId.ToString(), "accountInfo", accountInfo);
                if (con.AccountInfo.Value == default)
                    con.AccountInfo = await SelfUpdatingValue<AccountInfo>.Create(data.UserId.ToString(), "accountInfo");
            }
            catch (Exception e)
            {
                data.LogError(e, "updating accountInfo");
            }
        }

        public static async Task UpdateSettings(MessageData data, FlipSettings settings, int userId)
        {
            var service = DiHandler.ServiceProvider.GetRequiredService<SettingsService>();
            var con = (data as SocketMessageData).Connection;
            try
            {
                if (settings != null)
                    await service.UpdateSetting(userId.ToString(), "flipSettings", settings);
                if (con.FlipSettings.Value == default)
                    con.FlipSettings = await SelfUpdatingValue<FlipSettings>.Create(userId.ToString(), "flipSettings");
                if (settings == null)
                    await data.SendBack(data.Create("flipSettings", con.FlipSettings.Value));
            }
            catch (Exception e)
            {
                data.LogError(e, "updating flipsettings");
                con.OldFallbackSettings = settings;
            }
        }

        private static AccountInfo SettingsToAccountInfo(SettingsChange lastSettings)
        {
            return new AccountInfo()
            {
                ConIds = lastSettings.ConIds,
                ExpiresAt = lastSettings.ExpiresAt,
                McIds = lastSettings.McIds,
                Tier = lastSettings.Tier,
                UserId = lastSettings.UserId
            };
        }

        private static FlipSettings GetSettings(MessageData data)
        {
            FlipSettings settings = new FlipSettings();
            try
            {
                settings = data.GetAs<FlipSettings>();
                if(settings == null)
                    return null; // special case to load settings
                // test if settings compile
                settings.MatchesSettings(new FlipInstance()
                {
                    Auction = new SaveAuction()
                    {
                        Enchantments = new System.Collections.Generic.List<Enchantment>(),
                        FlatenedNBT = new System.Collections.Generic.Dictionary<string, string>(),
                        NBTLookup = new System.Collections.Generic.List<NBTLookup>()
                    },
                    LastKnownCost = 1
                });
            }
            catch (CoflnetException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                // could not get it continue with default
                data.LogError(e, "subFlip");
                throw new CoflnetException("invalid_settings", "Your settings are invalid, please revert your last change");
            }
            return settings;

        }
    }
}
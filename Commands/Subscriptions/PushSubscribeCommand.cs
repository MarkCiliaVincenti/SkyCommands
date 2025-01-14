using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MessagePack;
using Newtonsoft.Json;
using RestSharp;
using Coflnet.Sky.Core;
using Coflnet.Sky.Commands.Shared;
using Microsoft.Extensions.DependencyInjection;

namespace Coflnet.Sky.Commands
{
    public class PushSubscribeCommand : Command
    {
        public override async Task Execute(MessageData data)
        {
            var args = data.GetAs<Arguments>();

            var user = data.User;
            var userId = user.Id;
            List<SubscribeItem> subscriptions = (await SubscribeClient.GetSubscriptions(userId)).subscriptions;

            if (subscriptions.Count() >= 3)
            {
                var hasPremium = await DiHandler.ServiceProvider.GetService<PremiumService>().HasPremium(data.UserId);
                if (!hasPremium)
                    throw new NoPremiumException("Nonpremium users can only have 3 subscriptions");
            }

            var request = new RestRequest("Subscription/{userId}/sub", Method.Post)
                .AddJsonBody(new SubscribeItem()
                {
                    Type = args.Type,
                    TopicId = args.Topic,
                    Price = args.Price,
                    Filter = args.Filter
                })
                .AddUrlSegment("userId", user.Id);
            var response = await SubscribeClient.Client.ExecuteAsync(request);
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                throw new CoflnetException("subscribe_failed", "Your subscription could ot be saved");
            await data.Ok();
        }


        [MessagePackObject]
        public class Arguments
        {
            [Key("price")]
            public long Price;
            [Key("topic")]
            public string Topic;
            [Key("type")]
            public SubscribeItem.SubType Type;
            [Key("filter")]
            public string Filter;
        }
    }
}
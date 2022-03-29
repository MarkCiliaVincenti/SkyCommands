using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using Coflnet.Sky.Commands;
using Newtonsoft.Json;
using RestSharp;
using Coflnet.Sky.Core;

namespace Coflnet.Sky.Commands
{
    public class AccountInfoCommand : Command
    {
        public override async Task Execute(MessageData data)
        {
            var user = data.User;
            var token = LoginExternalCommand.GenerateToken(user.Email);
            var mcName = "unkown";

            var activeAccount = await McAccountService.Instance.GetActiveAccount(user.Id);
                    
            if (activeAccount != null && activeAccount.AccountUuid != null)
                mcName = await PlayerSearch.Instance.GetNameWithCacheAsync(activeAccount.AccountUuid);
            await data.SendBack(data.Create("acInfo", new Response(user.Email, token, activeAccount?.AccountUuid, mcName)));
        }

        [DataContract]
        public class Response
        {
            [DataMember(Name = "email")]
            public string Email;
            [DataMember(Name = "token")]
            public string Token;
            [DataMember(Name = "mcId")]
            public string MinecraftId;
            [DataMember(Name = "mcName")]
            public string MinecraftName;

            public Response()
            {
            }

            public Response(string email, string token, string minecraftId, string mcName)
            {
                Email = email;
                Token = token;
                MinecraftId = minecraftId;
                MinecraftName = mcName;
            }

            
        }
    }
}

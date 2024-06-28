using sodoffmmo.Attributes;
using sodoffmmo.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sodoffmmo.Management.Commands
{
    [ManagementCommand("setuserbuddycode", Role.Admin)]
    class SetBuddyCodeOfUserCommand : IManagementCommand
    {
        public void Handle(Client client, string[] arguments)
        {
            if (Configuration.ServerConfiguration.Authentication == false || Configuration.ServerConfiguration.ApiUrl == null) return;

            if (arguments.Length < 2) return; // just dont even do anything, im done with this lmao

            HttpClient httpClient = new();

            FormUrlEncodedContent changeRequest = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "token", client.PlayerData.UNToken },
                { "userId", arguments[0]},
                { "code", arguments[1]}
            });

            var changeResponse = httpClient.PostAsync($"{Configuration.ServerConfiguration.ApiUrl}/MMOAdmin/ChangeUserFriendCode", changeRequest)?.Result;
            if (changeResponse != null && changeResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                client.Send(Utils.ArrNetworkPacket(new string[] { "SMM", "-1", "SUCCESS", "User's Code Changed Successfully", "1" }, "SMM"));
            } else
            {
                client.Send(Utils.ArrNetworkPacket(new string[] { "SMM", "-1", "FAILIURE", "Failed To Change User's Code", "1" }, "SMM"));
            }
        }
    }
}

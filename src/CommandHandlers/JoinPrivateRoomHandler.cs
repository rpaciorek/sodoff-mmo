using sodoffmmo.Attributes;
using sodoffmmo.Core;
using sodoffmmo.Data;
using System.Runtime.Serialization.Formatters;
using System.Text.Json;

namespace sodoffmmo.CommandHandlers;

[ExtensionCommandHandler("JO")]
class JoinPrivateRoomHandler : CommandHandler
{
    public override Task Handle(Client client, NetworkObject receivedObject)
    {
        var p = receivedObject.Get<NetworkObject>("p");
        string roomName = p.Get<string>("rn") + "_" + p.Get<string>("0");
        Room room = Room.GetOrAdd(roomName, autoRemove: true);
        client.SetRoom(room);

        if(client.Room != null)
        {
            HttpClient httpClient = new();
            var locationSetRequest = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "token", client.PlayerData.UNToken },
                { "roomId", client.Room.Id.ToString() },
                { "roomName", p.Get<string>("rn") },
                { "isPrivate", "True" }
            });
            HttpResponseMessage? locationSetResponse = null;
            if (Configuration.ServerConfiguration.Authentication == AuthenticationMode.Required && Configuration.ServerConfiguration.ApiUrl != null)
                locationSetResponse = httpClient.PostAsync($"{Configuration.ServerConfiguration.ApiUrl}/MMO/SetBuddyLocation", locationSetRequest).Result;

            if (locationSetResponse != null && locationSetResponse.StatusCode == System.Net.HttpStatusCode.OK)
            {
                Console.WriteLine($"User {client.PlayerData.DiplayName}'s Location Is Now Set (Private Room)");
            }
        }

        return Task.CompletedTask;
    }
}

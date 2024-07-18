using sodoffmmo.Attributes;
using sodoffmmo.Core;
using sodoffmmo.Data;
using System.Text.Json;

namespace sodoffmmo.CommandHandlers;

[ExtensionCommandHandler("JA")]
class JoinRoomHandler : CommandHandler
{
    public override Task Handle(Client client, NetworkObject receivedObject)
    {
        string roomName = receivedObject.Get<NetworkObject>("p").Get<string>("rn");
        if (roomName is null) {
            roomName = client.PlayerData.ZoneName;
        }
        Room room = Room.GetOrAdd(roomName);
        client.SetRoom(room);
        if (client.Room == null) return Task.CompletedTask;

        // set current location of user to the room id

        HttpClient httpClient = new();
        var locationSetRequest = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "token", client.PlayerData.UNToken },
            { "roomId", client.Room.Id.ToString() },
            { "roomName", client.Room.Name.ToString() },
            { "isPrivate", "False" }
        });
        HttpResponseMessage? locationSetResponse = null;
        if (Configuration.ServerConfiguration.Authentication == AuthenticationMode.Required && Configuration.ServerConfiguration.ApiUrl != null)
            locationSetResponse = httpClient.PostAsync($"{Configuration.ServerConfiguration.ApiUrl}/MMO/SetBuddyLocation", locationSetRequest).Result;

        if(locationSetResponse != null && locationSetResponse.StatusCode == System.Net.HttpStatusCode.OK)
        {
            Console.WriteLine($"User {client.PlayerData.DiplayName}'s Location Is Now Set");
        }

        return Task.CompletedTask;
    }
}

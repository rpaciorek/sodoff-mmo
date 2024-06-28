using sodoffmmo.Attributes;
using sodoffmmo.Core;
using sodoffmmo.Data;
using sodoffmmo.Management;

namespace sodoffmmo.CommandHandlers;

[CommandHandler(1)]
class LoginHandler : CommandHandler
{
    public override Task Handle(Client client, NetworkObject receivedObject)
    {
        client.PlayerData.UNToken = receivedObject.Get<string>("un");
        client.PlayerData.ZoneName = receivedObject.Get<string>("zn");
        
        if (!ValidToken(client)) {
            NetworkObject obj = new();
            obj.Add("dr", (byte)1);
            client.Send(NetworkObject.WrapObject(0, 1005, obj).Serialize());
            client.ScheduleDisconnect();
            return Task.CompletedTask;
        }

        NetworkArray rl = new();

        NetworkArray r1 = new();
        r1.Add(0);
        r1.Add("MP_SYS");
        r1.Add("default");
        r1.Add(true);
        r1.Add(false);
        r1.Add(false);
        r1.Add((short)0);
        r1.Add((short)10);
        r1.Add(new NetworkArray());
        r1.Add((short)0);
        r1.Add((short)0);
        rl.Add(r1);

        NetworkArray r2 = new();
        r2.Add(1);
        r2.Add("ADMIN");
        r2.Add("default");
        r2.Add(false);
        r2.Add(false);
        r2.Add(true);
        r2.Add((short)0);
        r2.Add((short)1);
        r2.Add(WorldEvent.Get().EventInfoArray(true));
        rl.Add(r2);

        NetworkArray r3 = new();
        r3.Add(2);
        r3.Add("LIMBO");
        r3.Add("default");
        r3.Add(false);
        r3.Add(false);
        r3.Add(false);
        r3.Add((short)31);
        r3.Add((short)10000);
        r3.Add(new NetworkArray());
        rl.Add(r3);

        NetworkObject content = new();
        content.Add("rl", rl);
        content.Add("zn", client.PlayerData.ZoneName);
        content.Add("rs", (short)5);
        content.Add("un", client.PlayerData.UNToken);
        content.Add("id", client.ClientID);
        content.Add("pi", (short)1);

        client.Send(NetworkObject.WrapObject(0, 1, content).Serialize());
        return Task.CompletedTask;
    }

    private bool ValidToken(Client client) {
        if (Configuration.ServerConfiguration.Authentication == AuthenticationMode.Disabled ||
            (client.PlayerData.UNToken == Configuration.ServerConfiguration.BypassToken && !string.IsNullOrEmpty(Configuration.ServerConfiguration.BypassToken)))
            return true;

        try {
            HttpClient httpClient = new();
            var content = new FormUrlEncodedContent(
                new Dictionary<string, string> {
                    { "token", client.PlayerData.UNToken },
            });

            httpClient.Timeout = new TimeSpan(0, 0, 3);
            var response = httpClient.PostAsync($"{Configuration.ServerConfiguration.ApiUrl}/Authentication/MMOAuthentication", content).Result;
            string? responseString = response.Content.ReadAsStringAsync().Result;

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
                throw new Exception($"Response status code {response.StatusCode}");
            if (responseString == null)
                throw new Exception("Response string null");

            AuthenticationInfo info = Utils.DeserializeXml<AuthenticationInfo>(responseString);
            if (info.Authenticated) {
                client.PlayerData.DiplayName = info.DisplayName;
                client.PlayerData.Role = info.Role;

                // set user as online api side
                var onlineSetRequest = new FormUrlEncodedContent(
                new Dictionary<string, string> {
                    { "token", client.PlayerData.UNToken },
                    { "online", "true" }
                });

                HttpResponseMessage? onlineSetResponse = null;
                string? onlineResString = null;

                if (Configuration.ServerConfiguration.Authentication == AuthenticationMode.Required && Configuration.ServerConfiguration.ApiUrl != null)
                    onlineSetResponse = httpClient.PostAsync($"{Configuration.ServerConfiguration.ApiUrl}/MMO/SetBuddyOnline", onlineSetRequest).Result;
                
                if (onlineSetResponse != null)
                    onlineResString = onlineSetResponse.Content.ReadAsStringAsync().Result;

                if (onlineSetResponse.StatusCode == System.Net.HttpStatusCode.OK && onlineResString != null)
                {
                    Console.WriteLine($"User {client.PlayerData.DiplayName} Is Now Online");
                } else
                {
                    Console.WriteLine($"Was Unable To Set {client.PlayerData.DiplayName} As Online");
                }

                return true;
            }
        } catch (Exception ex) {
            Console.WriteLine($"Authentication exception IID: {client.ClientID} - {ex}");
        }
        return Configuration.ServerConfiguration.Authentication != AuthenticationMode.Required; // return true on auth err if not Required mode
    }
}

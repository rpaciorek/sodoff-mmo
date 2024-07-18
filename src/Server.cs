using sodoffmmo.API;
using sodoffmmo.Core;
using sodoffmmo.Data;
using sodoffmmo.Management;
using System;
using System.Net;
using System.Net.Sockets;

namespace sodoffmmo;
public class Server {

    readonly int port;
    readonly IPAddress ipAddress;
    readonly bool IPv6AndIPv4;
    ModuleManager moduleManager = new();

    public static List<Client> AllClients { get; set; } = new();

    public Server(IPAddress ipAdress, int port, bool IPv6AndIPv4) {
        this.ipAddress = ipAdress;
        this.port = port;
        this.IPv6AndIPv4 = IPv6AndIPv4;
    }

    public async Task Run() {
        moduleManager.RegisterModules();
        ManagementCommandProcessor.Initialize();
        using Socket listener = new(ipAddress.AddressFamily,
                                    SocketType.Stream,
                                    ProtocolType.Tcp);
        if (IPv6AndIPv4)
            listener.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.IPv6Only, 0);
        listener.Bind(new IPEndPoint(ipAddress, port));
        Room.GetOrAdd("LoungeInt").AddAlert(new Room.AlertInfo("3")); // FIXME use config for this
        Room.GetOrAdd("Spaceport").AddAlert(new Room.AlertInfo("1", 20.0, 300, 300));
        Room.GetOrAdd("Spaceport").AddAlert(new Room.AlertInfo("2", 120.0, 1800, 3600));
        Room.GetOrAdd("Academy").AddAlert(new Room.AlertInfo("1", 20.0, 300, 300));
        var apiBuilder = CreateApiBuilder().Build();
        await apiBuilder.StartAsync(); // start minimal api for api side updates and events
        await Listen(listener);

        // when listener exits, stop and dispose api
        await apiBuilder.StopAsync();
        apiBuilder.Dispose();
    }

    public static IHostBuilder CreateApiBuilder()
    {
        return Host.CreateDefaultBuilder().ConfigureWebHostDefaults(webHost =>
        {
            webHost.UseUrls($"http://*:{Configuration.ServerConfiguration.HttpApiPort}");
            webHost.UseStartup<ApiStartup>();
        });
    }

    private async Task Listen(Socket listener) {
        Console.WriteLine($"MMO Server listening on port {port}");
        listener.Listen(100);
        while (true) {
            Socket handler = await listener.AcceptAsync();
            handler.SendTimeout = 200;
            Console.WriteLine($"New connection from {((IPEndPoint)handler.RemoteEndPoint!).Address}");
            _ = Task.Run(() => HandleClient(handler));
        }
    }

    private async Task HandleClient(Socket handler) {
        Client client = new(handler);
        AllClients.Add(client);
        try {
            while (client.Connected) {
                await client.Receive();
                List<NetworkObject> networkObjects = new();
                while (client.TryGetNextPacket(out NetworkPacket packet))
                    networkObjects.Add(packet.GetObject());

                await HandleObjects(networkObjects, client);
            }
        } finally {
            try {
                client.SetRoom(null);
                AllClients.Remove(client);
            } catch (Exception) { }

            // set user as offline and blank out current location
            HttpClient httpClient = new();
            var onlineSetRequest = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "token", client.PlayerData.UNToken },
                { "online", "false" }
            });
            var locationSetRequest = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "token", client.PlayerData.UNToken },
                { "roomId", string.Empty },
                { "roomName", string.Empty },
                { "isPrivate", "False" }
            });

            var onlineSetResponse = httpClient.PostAsync($"{Configuration.ServerConfiguration.ApiUrl}/MMO/SetBuddyOnline", onlineSetRequest).Result;
            string? onlineResString = onlineSetResponse.Content.ReadAsStringAsync().Result;
            var locationSetResponse = httpClient.PostAsync($"{Configuration.ServerConfiguration.ApiUrl}/MMO/SetBuddyLocation", locationSetRequest).Result;

            if (onlineSetResponse.StatusCode == HttpStatusCode.OK && onlineResString != null)
            {
                Console.WriteLine($"User {client.PlayerData.DiplayName} Is Now Offline");
                if(locationSetResponse.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine($"User {client.PlayerData.DiplayName}'s Location Is Now Empty");
                }
            }
            else
            {
                Console.WriteLine($"Was Unable To Set {client.PlayerData.DiplayName} As Offline");
            }

            client.Disconnect();
            Console.WriteLine("Socket disconnected IID: " + client.ClientID);
        }
    }

    private async Task HandleObjects(List<NetworkObject> networkObjects, Client client) {
        foreach (var obj in networkObjects) {
            try {
                short commandId = obj.Get<short>("a");
                CommandHandler handler;
                if (commandId != 13) {
                    if (commandId == 0 || commandId == 1)
                        Console.WriteLine($"System command: {commandId} IID: {client.ClientID}");
                    handler = moduleManager.GetCommandHandler(commandId);
                } else
                    handler = moduleManager.GetCommandHandler(obj.Get<NetworkObject>("p").Get<string>("c"));
                Task task = handler.Handle(client, obj.Get<NetworkObject>("p"));
                if (!handler.RunInBackground)
                    await task;
            } catch (Exception ex) {
                Console.WriteLine($"Exception IID: {client.ClientID} - {ex}");
            }
        }
    }
}

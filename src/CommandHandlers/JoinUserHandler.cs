using sodoffmmo.Attributes;
using sodoffmmo.Core;
using sodoffmmo.Data;

namespace sodoffmmo.CommandHandlers
{
    [ExtensionCommandHandler("JU")]
    class JoinUserHandler : CommandHandler
    {
        public override Task Handle(Client client, NetworkObject receivedObject)
        {
            string mpId = receivedObject.Get<NetworkObject>("p").Get<string>("0");

            // here we're just using "MultiplayerID" as the room id, set the clients room to that id
            Room? room = Room.AllRooms().FirstOrDefault(e => e.Id == Convert.ToInt32(mpId));

            if (room != null)
            {
                client.SetRoom(room);
            }
            else client.SetRoom(client.Room); // set room back to current room if room does not exist

            return Task.CompletedTask;
        }
    }
}

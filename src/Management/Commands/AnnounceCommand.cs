using sodoffmmo.Attributes;
using sodoffmmo.Core;

namespace sodoffmmo.Management.Commands;

[ManagementCommand("announce", Role.Admin)]
class AnnounceCommand : IManagementCommand {
    public void Handle(Client client, string[] arguments) {
        if (arguments.Length == 0) {
            client.Send(Utils.BuildServerSideMessage("Announce: No message to announce", "Server"));
            client.Send(Utils.ArrNetworkPacket(new string[] { "SMM", "-1", "NO_MESSAGE", "No Message Provided", "1" }, "SMM"));
            return;
        }

        // send both SoD announcement and legacy announcement (no way to tell what client they are on)
        client.Room.Send(Utils.BuildServerSideMessage(string.Join(' ', arguments), "Server"));
        client.Room.Send(Utils.ArrNetworkPacket(new string[] { "SMM", "-1", "ANNOUNCEMENT", string.Join(' ', arguments), "1" }, "SMM"));
    }
}

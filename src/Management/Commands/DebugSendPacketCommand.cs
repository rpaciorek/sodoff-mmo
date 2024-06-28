using sodoffmmo.Attributes;
using sodoffmmo.Core;

namespace sodoffmmo.Management.Commands;

[ManagementCommand("dbgsendarrpacket", Role.Admin)]
class DebugSendPacketCommand : IManagementCommand
{
    public void Handle(Client client, string[] arguments)
    {
        if(arguments.Length < 0)
        {
            client.Send(Utils.ArrNetworkPacket(new string[] { "SMM", "-1", "NO_ARGS", "No Arguments Were Provided", "1" }, "SMM"));
            return;
        } else if (arguments.Length <= 1)
        {
            client.Send(Utils.ArrNetworkPacket(new string[] { "SMM", "-1", "INVALID_ARGS", "Not Enough Arguments Provided", "1" }, "SMM"));
            return;
        }

        List<string> cmdArgs = new List<string>();

        for(int i = 0; i < arguments.Length; i++)
        {
            if (i == 0) continue; // do not put command into command args
            cmdArgs.Add(arguments[i]);
        }

        // TODO: implement a way to send object to specific clients
        if (client.Room == null) client.Send(Utils.ArrNetworkPacket(new string[] { "SMM", "-1", "NOT_IN_ROOM", "You Are Currently Not In A Room", "1" }, "SMM"));
        else { client.Room.Send(Utils.ArrNetworkPacket(cmdArgs.ToArray(), arguments[0])); client.Send(Utils.ArrNetworkPacket(new string[] { "SCA", "-1", "1", "Packet Sent To All Users", "", "1" }, "SCA")); }
    }
}

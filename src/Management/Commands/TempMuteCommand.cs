using sodoffmmo.Attributes;
using sodoffmmo.Core;

namespace sodoffmmo.Management.Commands;

[ManagementCommand("tempmute", Role.Moderator)]
class TempMuteCommand : IManagementCommand {
    public void Handle(Client client, string[] arguments) {
        if (arguments.Length != 1) {
            client.Send(Utils.BuildServerSideMessage("TempMute: Invalid number of arguments", "Server"));
            client.Send(Utils.ArrNetworkPacket(new string[] { "SMM", "-1", "INVALID_ARG_NUMBER", "Not Enough Arguments", "1" }, "SMM"));
            return;
        }
        Client? target = client.Room.Clients.FirstOrDefault(x => x.PlayerData.DiplayName == arguments[0]);
        if (target == null) {
            client.Send(Utils.BuildServerSideMessage($"TempMute: user {arguments[0]} not found", "Server"));
            client.Send(Utils.ArrNetworkPacket(new string[] { "SMM", "-1", "USER_NOT_FOUND", "User not found.", "1" }, "SMM"));
            return;
        }
        target.TempMuted = !target.TempMuted;
        if (target.TempMuted)
        {
            client.Send(Utils.BuildServerSideMessage($"TempMute: {arguments[0]} has been temporarily muted", "Server"));
            client.Send(Utils.ArrNetworkPacket(new string[] { "SMM", "-1", "MUTED", $"{arguments[0]} Has Been Temporarily Muted.", "1" }, "SMM"));
            target.Send(Utils.ArrNetworkPacket(new string[] { "SMM", "-1", "MUTED", $"You Have Been Temporarily Muted By A Moderator. This Means You Can No Longer Chat Until You Are Unmuted. Repeated Offences Could Get You Banned.", "1" }, "SMM"));
        }
        else
        {
            client.Send(Utils.BuildServerSideMessage($"TempMute: {arguments[0]} has been unmuted", "Server"));
            client.Send(Utils.ArrNetworkPacket(new string[] { "SMM", "-1", "UNMUTED", $"{arguments[0]} Has Been Unmuted.", "1" }, "SMM"));
        }
    }
}

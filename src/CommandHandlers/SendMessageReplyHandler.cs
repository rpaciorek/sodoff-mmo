using sodoffmmo.Attributes;
using sodoffmmo.Core;
using sodoffmmo.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sodoffmmo.CommandHandlers;

[ExtensionCommandHandler("SMR")]
class SendMessageReplyHandler : CommandHandler
{
    public override Task Handle(Client client, NetworkObject receivedObject)
    {
        //client.Send(Utils.ArrNetworkPacket(new string[] { "SMF" }, "SMF"));
        //client.Send(Utils.ArrNetworkPacket(new string[] { "SMM", "-1", "DISABLED", "At this time, replies to messages are disabled due to a potential client bug. Sorry :(" }));

        NetworkObject args = receivedObject.Get<NetworkObject>("p");

        string token = args.Get<string>("tkn");
        string toUserId = args.Get<string>("tgt");
        string content = args.Get<string>("cnt");
        string level = args.Get<string>("lvl");
        string msgId = args.Get<string>("rtm");

        if (toUserId == string.Empty) toUserId = client.PlayerData.Uid; // send to self

        HttpClient httpClient = new();

        var postMsgRequest = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "token", token },
            { "toUserId", toUserId },
            { "content", content },
            { "level", level },
            { "replyMsgId", msgId }
        });

        HttpResponseMessage? postMsgResponse = null;
        if (Configuration.ServerConfiguration.Authentication == AuthenticationMode.Required && Configuration.ServerConfiguration.ApiUrl != null) postMsgResponse = httpClient.PostAsync($"{Configuration.ServerConfiguration.ApiUrl}/MMO/SendMessage", postMsgRequest).Result;

        if (postMsgResponse != null && postMsgResponse.StatusCode == System.Net.HttpStatusCode.OK)
        {
            client.Send(Utils.ArrNetworkPacket(new string[] { "SMA", "-1", "SUCCESS", "1", DateTime.UtcNow.ToString() }, "SMA"));
        }
        else
        {
            client.Send(Utils.ArrNetworkPacket(new string[] { "SMF" }, "SMF"));
        }

        // refresh all users in room message queue
        if (client.Room is not null) client.Room.Send(Utils.ArrNetworkPacket(new string[] { "SPMN" }, "SPMN"));

        return Task.CompletedTask;
    }
}

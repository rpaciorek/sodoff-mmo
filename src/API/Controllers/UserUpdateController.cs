using Microsoft.AspNetCore.Mvc;
using sodoffmmo.Core;
using sodoffmmo.Data;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace sodoffmmo.API.Controllers
{
    [ApiController]
    [Route("mmo/update")]
    public class UserUpdateController : ControllerBase
    {
        [HttpPost]
        [Route("SendPacketToRoom")]
        public IActionResult SendPacketToRoom([FromForm] string apiToken, [FromForm] string roomName, [FromForm] string cmd, [FromForm] string serializedArgs)
        {
            Client? client = Server.AllClients.FirstOrDefault(e => e.PlayerData.UNToken == apiToken);
            Room? room = Room.Get(roomName);

            if (client == null) return Unauthorized();
            else if (room == null) return BadRequest("Invalid Room Name Or Room Doesn't Exist Yet");

            // send packet
            room.Send(Utils.ArrNetworkPacket(JsonSerializer.Deserialize<string[]>(serializedArgs), cmd));

            return Ok();
        }

        [HttpPost]
        [Route("SendPacketToPlayer")]
        public IActionResult SendPacketToPlayer([FromForm] string apiToken, [FromForm] string userId, [FromForm] string cmd, [FromForm] string serializedArgs)
        {
            Client? client = Server.AllClients.FirstOrDefault(e => e.PlayerData.UNToken == apiToken);
            Client? receivingClient = Server.AllClients.FirstOrDefault(e => e.PlayerData.Uid == userId);

            if (client == null) return Unauthorized();

            if (receivingClient != null) receivingClient.Send(Utils.ArrNetworkPacket(JsonSerializer.Deserialize<string[]>(serializedArgs), cmd));
            else return BadRequest("User Not Found");

            return Ok();
        }

        [HttpPost]
        [Route("UpdateRoomVarsInRoom")]
        public IActionResult UpdateRoomVarsInRoom([FromForm] string apiToken, [FromForm] string roomName, [FromForm] string serializedVars)
        {
            Dictionary<string, string>? vars = JsonSerializer.Deserialize<Dictionary<string, string>>(serializedVars);
            Room? room = Room.Get(roomName);
            Client? client = Server.AllClients.FirstOrDefault(e => e.PlayerData.UNToken == apiToken);

            if (client == null) return Unauthorized();

            if (vars != null && room != null)
            {
                if (room.RoomVariables == null) room.RoomVariables = new();
                foreach (var rVar in vars)
                {
                    room.RoomVariables.Add(NetworkArray.VlElement(rVar.Key, rVar.Value, isPersistent: true));
                }
                NetworkPacket packet = Utils.VlNetworkPacket(room.RoomVariables, room.Id);
                room.Send(packet);
                return Ok();
            }
            else return BadRequest("Room Not Found Or Vars Are Empty");
        }
    }
}

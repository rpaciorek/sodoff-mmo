using Microsoft.AspNetCore.Mvc;
using sodoffmmo.Core;

namespace sodoffmmo.API.Controllers
{
    [ApiController]
    [Route("mmo/update")]
    public class UserUpdateController : ControllerBase
    {
        [HttpPost]
        [Route("SendUpdatePacketToRoom")]
        public IActionResult SendUpdatePacketToRoom([FromForm] string apiToken, [FromForm] string roomName, [FromForm] string cmd, [FromForm] string[] args)
        {
            Client? client = Server.AllClients.FirstOrDefault(e => e.PlayerData.UNToken == apiToken);
            Room? room = Room.Get(roomName);

            if (client == null) return Unauthorized();
            else if (room == null) return BadRequest("Invalid Room Name Or Room Doesn't Exist Yet");

            // send packet
            room.Send(Utils.ArrNetworkPacket(args, cmd));

            return Ok();
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using sodoffmmo.Core;

namespace sodoffmmo.API.Controllers
{
    [ApiController]
    [Route("mmo/stats")]
    public class StatisticsController : ControllerBase
    {
        [HttpGet]
        [Route("GetNumberOfPlayersFromRoom")]
        public IActionResult GetNumberOfPlayersFromRoom(string name)
        {
            Room room = Room.Get(name);
            if (room != null) return Ok(room.Clients.Count());
            else return BadRequest("Invalid Room Name Or Room Doesn't Exist Yet");
        }

        [HttpGet]
        [Route("GetRoomList")]
        public IActionResult GetRoomList()
        {
            List<Room> allRooms = Room.AllRooms().ToList();
            string[] strings = new string[allRooms.Count];

            var i = 0;
            foreach(var room in allRooms)
            {
                strings[i] = room.Name;
                i++;
            }

            return Ok(strings);
        }

        [HttpGet]
        [Route("GetCombinedNumberOfUsersOnline")]
        public IActionResult GetCombinedNumberOfUsersOnline()
        {
            return Ok(Server.AllClients.Count);
        }
    }
}

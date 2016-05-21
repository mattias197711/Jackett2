using Jackett.Irc.Models;
using Jackett.Irc.Models.DTO;
using Jackett2.Irc.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jackett2.Controllers
{
    [Route("/ircchannel")]
    [Produces("application/json")]
    public class IRCChannelController : Controller
    {
        private IIRCService irc;

        public IRCChannelController(IIRCService i)
        {
            irc = i;
        }

        [HttpGet("Messages/{network}")]
        public List<Message> Messages(string network)
        {
            return irc.GetMessages(network, null);
        }

        [HttpGet("Messages/{network}/{room}")]
        public List<Message> Messages(string network, string room)
        {
            return irc.GetMessages(network, room);
        }

        [HttpGet("Users/{network}/{room}")]
        public List<User> Users(string network, string room)
        {
            return irc.GetUser(network, room);
        }

        [HttpPost]
        public IActionResult Command([FromBody]IRCommandDTO command)
        {
            irc.ProcessCommand(command.NetworkId, command.ChannelId, command.Text);
            return Ok();
        }
    }
}

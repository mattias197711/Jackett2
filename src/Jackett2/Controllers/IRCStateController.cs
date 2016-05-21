using Jackett.Irc.Models.DTO;
using Jackett2.Irc.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jackett2.Controllers
{
    [Route("/ircstate")]
    [Produces("application/json")]
    public class IRCStateController : Controller
    {
        private IIRCService irc;

        public IRCStateController(IIRCService i)
        {
            irc = i;
        }

        [HttpGet]
        public List<NetworkDTO> Get()
        {
            return irc.GetSummary();
        }
    }
}

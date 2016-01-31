using Jackett.Irc.Models;
using Jackett2.Irc.Services;
using Microsoft.AspNet.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jackett2.Controllers
{
    [Route("/ircchannel")]
    [Produces("application/json")]
    public class IRCProfileController : Controller
    {
        IIRCProfileService profileService;

        public IRCProfileController(IIRCProfileService p)
        {
            profileService = p;
        }

        [HttpGet]
        public List<IRCProfile> Index()
        {
            return profileService.All;
        }

        [HttpGet("{id}")]
        public IActionResult Get(string id)
        {
            var item = profileService.Get(id);
            if (item == null)
                return HttpNotFound();
            return Json(item);
        }

        [HttpPut]
        public IActionResult Put([FromBody]IRCProfile profile)
        {
            profileService.Set(profile);
            return Ok();
        }

        [HttpDelete]
        public IActionResult Delete(string id)
        {
            profileService.Delete(id);
            return Ok();
        }
    }
}

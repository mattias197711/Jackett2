using Jackett.Irc.Models.AutoDL;
using Jackett2.Irc.Services;
using Microsoft.AspNet.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Jackett2.Controllers
{
    public class AutoDLController : Controller
    {
        IAutoDLProfileService autodlService;

        public AutoDLController(IAutoDLProfileService a)
        {
            autodlService = a;
        }

        //[HttpGet]
        //[Route("AutoDL/Summary")]
        public List<NetworkSummary> Summary()
        {
            return autodlService.GetNetworks();
        }

      //  [HttpGet]
        public List<AutoDLProfileSummary> Index()
        {
            return autodlService.GetProfiles();
        }

       // [HttpPut]
        public IActionResult Put([FromBody]AutoDLProfileSummary profile)
        {
            autodlService.Set(profile);
            return Ok();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackett.Irc.Models.AutoDL
{
    public class SavedAutoDLConfig
    {
        public string Type { set; get; }
        public Dictionary<string, string> Options { get; set; } = new Dictionary<string, string>();
    }
}

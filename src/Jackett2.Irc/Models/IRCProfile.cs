﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackett.Irc.Models
{
    public class IRCProfile
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<string> Servers { get; set; } = new List<string>();
        public string Username { get; set; }
        public string Profile { get; set; }
    }
}

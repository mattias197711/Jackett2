﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackett.Irc.Models
{
    public class IRCProfiles
    {
        public List<IRCProfile> Profiles { set; get; } = new List<IRCProfile>();
    }
}

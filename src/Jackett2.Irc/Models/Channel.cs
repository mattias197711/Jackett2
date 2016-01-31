﻿using IrcDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackett.Irc.Models
{
    public class Channel
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public List<Message> Messages { get; } = new List<Message>();
        public bool Joined { get; set; }
    }
}

using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackett.Irc.Models.Command
{
    public class AddProfileEvent : INotification
    {
        public IRCProfile Profile { get; set; }
    }
}

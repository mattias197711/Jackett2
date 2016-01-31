using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Jackett2.Irc.Services;

namespace Jackett2.Irc
{
    public class JackettIrcModule
    {
        public static void Register(IServiceCollection collection)
        {
            collection.AddSingleton<IIRCService, IRCService>();
            collection.AddSingleton<IAutoDLProfileService, AutoDLProfileService>();
            collection.AddSingleton<IIRCProfileService, IRCProfileService>();
        }
    }
}

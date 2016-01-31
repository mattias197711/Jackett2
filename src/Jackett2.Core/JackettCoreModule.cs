using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Jackett2.Core.Services;

namespace Jackett2.Irc
{
    public class JackettCoreModule
    {
        public static void Register(IServiceCollection collection)
        {
            collection.AddSingleton<IConfigurationService, ConfigurationService>();
            collection.AddSingleton<IIDService, IDService>();
            collection.AddSingleton<ISerializeService, SerializeService>();
        }
    }
}

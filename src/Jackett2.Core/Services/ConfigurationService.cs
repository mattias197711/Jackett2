using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Extensions.Logging;

namespace Jackett2.Core.Services
{
    public interface IConfigurationService
    {
        string ApplicationFolder();
        string GetAutoDLFolder();
        void SaveConfig<T>(T config);
        T GetConfig<T>();
    }

    public class ConfigurationService : IConfigurationService
    {
        IHostingEnvironment hosting;
        IHostingEnvironment app;
        ISerializeService serializeService;
        ILogger<ConfigurationService> logger;

        public ConfigurationService(IHostingEnvironment h, IHostingEnvironment a, ISerializeService s, ILogger<ConfigurationService> l)
        {
            logger = l;
            hosting = h;
            serializeService = s;
            app = a;
        }

        public string ApplicationFolder()
        {
            return app.ContentRootPath;
        }

        public string GetAutoDLFolder()
        {
            // If we are debugging we can use the non copied content.
            string dir = Path.GetFullPath(Path.Combine(ApplicationFolder(), "autodl-trackers"));
            return dir;
        }

        public T GetConfig<T>()
        {
            var type = typeof(T);
            var fullPath = Path.Combine(GetAppDataFolder(), type.Name + ".json");
            try
            {
                if (!File.Exists(fullPath))
                {
                    logger.LogDebug("Config file does not exist: " + fullPath);
                    return default(T);
                }

                return serializeService.DeSerialise<T>(File.ReadAllText(fullPath));
            }
            catch (Exception e)
            {
                logger.LogError("Error reading config file " + fullPath, e);
                return default(T);
            }
        }

        public string GetAppDataFolder()
        {
            return Path.Combine(ApplicationFolder(), "Jackett2Data");
        }

        public void SaveConfig<T>(T config)
        {
            var type = typeof(T);
            var fullPath = Path.Combine(GetAppDataFolder(), type.Name + ".json");
            try
            {
                var json = serializeService.Serialise(config);
                if (!Directory.Exists(GetAppDataFolder()))
                    Directory.CreateDirectory(GetAppDataFolder());
                File.WriteAllText(fullPath, json);
            }
            catch (Exception e)
            {
                logger.LogError("Error reading config file " + fullPath, e);
            }
        }
    }
}

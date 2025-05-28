
using DmarcTlsReportParser.Models;
using Microsoft.Extensions.Configuration;

namespace DmarcTlsReportParser
{
    public class ConfigLoader
    {
        public static AppConfig Load(string path = "config.json")
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile(path, optional: false, reloadOnChange: false)
                .Build();

            var appConfig = new AppConfig();
            config.Bind(appConfig);
            return appConfig;
        }

    }
}

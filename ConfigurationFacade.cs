using Microsoft.Extensions.Configuration;

namespace simple.dbapp
{
    public class ConfigurationFacade : IConfigurationFacade
    {
        private IConfiguration Configuration;
        public ConfigurationFacade(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public T GetValue<T>(string key)
        {
            return Configuration.GetValue<T>(key);
        }

        public string GetConnectionString(string key)
        {
            return Configuration.GetConnectionString(key);
        }
    }

    public interface IConfigurationFacade
    {
        T GetValue<T>(string key);
        string GetConnectionString(string key);
    }
}
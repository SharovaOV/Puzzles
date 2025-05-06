using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Puzzles
{
    public static class ConfigurationLoader
    {
        public static IConfiguration LoadConfiguration()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            return builder.Build();
        }

        public static T GetSection<T>(this IConfiguration configuration) where T : new()
        {
            var section = new T();
            
            configuration.GetSection(typeof(T).Name).Bind(section);
            return section;
        }
    }
}

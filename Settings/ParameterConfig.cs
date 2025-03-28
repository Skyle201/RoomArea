using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RoomArea.Settings
{
    public class ParameterConfig
    {

            public string sideWallsParameterName { get; set; } = "Комментарии";

            public static ParameterConfig Load()
            {
                string dllPath = Assembly.GetExecutingAssembly().Location;
                string pluginDir = Path.GetDirectoryName(dllPath);
            string configPath = Path.Combine(pluginDir, "Settings", "Config.json");

                if (!File.Exists(configPath))
                    throw new FileNotFoundException($"Файл конфигурации не найден: {configPath}");

                string json = File.ReadAllText(configPath);
                return JsonConvert.DeserializeObject<ParameterConfig>(json);
            }
        
    }
}

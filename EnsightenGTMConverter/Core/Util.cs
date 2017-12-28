using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnsightenGTMConverter.FileActions;
using System.IO;
using System.Configuration;
using System.Reflection;
using EnsightenGTMConverter.Models;

namespace EnsightenGTMConverter.Core
{
    public class Util
    {
        public static Configuration GetConfig(Assembly assembly)
        {
            string path = assembly.Location + ".config";

            if (!File.Exists(path))
                return null;

            return ConfigurationManager.OpenMappedExeConfiguration(new ExeConfigurationFileMap { ExeConfigFilename = path }, ConfigurationUserLevel.None);
        }

        public static string GetFileName(string file)
        {
            return Path.GetFileName(file.TrimEnd(Path.DirectorySeparatorCh‌​ar));
        }
    }
}

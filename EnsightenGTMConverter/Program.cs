using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using EnsightenGTMConverter.FileActions;
using EnsightenGTMConverter.Models;
using EnsightenGTMConverter.Core;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using Cql.Common.Logging;

namespace EnsightenGTMConverter
{
    class Program
    {
        private static readonly ILogger Logger = LogContextManager.GetLogger<FileManager>();
        static void Main(string[] args)
        {
            var config = Util.GetConfig(Assembly.GetExecutingAssembly());

            string importDir = config.AppSettings.Settings["ImportDirectory"].Value;
            string exportDir = config.AppSettings.Settings["ExportDirectory"].Value;
            string configFile = config.AppSettings.Settings["ConfigDirectory"].Value + "\\config.json";

            Config configJson = new Config();

            using (StreamReader file = File.OpenText(configFile))
            {
                JsonSerializer serializer = new JsonSerializer();
                configJson = (Config)serializer.Deserialize(file, typeof(Config));
            }

            var importSubDirs = new List<string>(Directory.GetDirectories(importDir));
            try
            {
                foreach (string subDir in importSubDirs)
                {
                    var files = Directory.GetFiles(subDir);
                    if (files.Length == 0)
                        return;

                    var dirName = Util.GetFileName(subDir);
                    var configDir = configJson.Directories.Where(d => d.Name == dirName).ToList()[0];

                    if (configDir == null)
                        continue;

                    foreach (ConfigContainer obj in configDir.Containers)
                    {
                        var gtm = new GTMObject();
                        gtm.exportFormatVersion = 2;
                        gtm.exportTime = DateTime.Now;

                        var cv = new ContainerVersion();
                        cv.container = new Container();
                        cv.accountId = configJson.AccountID;
                        cv.containerId = 1234567;
                        cv.container.publicId = obj.ContainerID;
                        cv.container.accountId = cv.accountId;
                        cv.container.containerId = cv.containerId;

                        foreach (var file in files)
                        {
                            var xlsReader = new XlsReader();
                            xlsReader.ReadXlsData(file, cv, obj);
                        }

                        gtm.containerVersion = cv;

                        //delete export file if exists
                        var exportFile = exportDir + '\\' + obj.Name + ".json";
                        if (File.Exists(exportFile))
                        {
                            File.Delete(exportFile);
                        }

                        //create export file 
                        var writer = new GtmJsonWriter();
                        writer.WriteJSON(exportFile, gtm);
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.Error(string.Format("Exception occurred importing and exporting."), ex);
            }
        }
    }
}

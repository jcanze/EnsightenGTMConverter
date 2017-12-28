using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using EnsightenGTMConverter.Models;

namespace EnsightenGTMConverter.FileActions
{
    public class GtmJsonWriter
    {
        public void WriteJSON(string file, GTMObject gtm)
        {
            using (StreamWriter sw = File.CreateText(file))
            {
                JsonSerializer serializer = new JsonSerializer{ NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented };
                serializer.Serialize(sw, gtm);
            }
        }
    }
}

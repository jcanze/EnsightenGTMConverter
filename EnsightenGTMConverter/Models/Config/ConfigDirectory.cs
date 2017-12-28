using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnsightenGTMConverter.Models
{
    public class ConfigDirectory
    {
        public string Name { get; set; }
        public List<ConfigContainer> Containers { get; set; }
    }
}

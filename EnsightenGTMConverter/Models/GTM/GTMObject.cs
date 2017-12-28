using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EnsightenGTMConverter.Models;

namespace EnsightenGTMConverter.Models
{
    public class GTMObject
    {
        public long exportFormatVersion { get; set; }
        public DateTime exportTime { get; set; }
        public ContainerVersion containerVersion { get; set; }
    }
}

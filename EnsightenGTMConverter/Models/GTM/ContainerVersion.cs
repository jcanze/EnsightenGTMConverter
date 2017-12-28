using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnsightenGTMConverter.Models
{
    public class ContainerVersion
    {
        public long accountId { get; set; }
        public long containerId { get; set; }
        public Container container { get; set; }
        public List<Tag> tag { get; set; }
        public List<Trigger> trigger { get; set; }
        public List<Variable> variable { get; set; }
    }
}

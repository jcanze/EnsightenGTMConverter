using System;
using System.Collections.Generic;

namespace EnsightenGTMConverter.Models
{
    public class Tag
    {
        public long accountId { get; set; }
        public long containerId { get; set; }
        public string tagId { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public List<Parameter> parameter { get; set; }
        public List<String> firingTriggerId { get; set; }
        public string tagFiringOption { get; set; }
    }
}

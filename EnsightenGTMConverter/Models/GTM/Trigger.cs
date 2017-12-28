using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnsightenGTMConverter.Models
{
    public class Trigger
    {
        public long accountId { get; set; }
        public long containerId { get; set; }
        public string triggerId { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public List<Filter> filter { get; set; }
    }
}

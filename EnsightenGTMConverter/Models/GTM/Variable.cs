using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnsightenGTMConverter.Models
{
    public class Variable
    {
        public long accountId { get; set; }
        public long containerId { get; set; }
        public string variableId { get; set; }
        public string name { get; set; }
        public string type { get; set; }
        public List<Parameter> parameter { get; set; }
    }
}

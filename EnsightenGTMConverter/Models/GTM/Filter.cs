using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnsightenGTMConverter.Models
{
    public class Filter
    {
        public string type { get; set; }
        public List<Parameter> parameter { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnsightenGTMConverter.Models
{
    public class Config
    {
        public long AccountID { get; set; }
        public List<ConfigDirectory> Directories { get; set; }
       
    }
}

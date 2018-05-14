using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlingBPlusTrees
{
    class Record
    {
        public Dictionary<string, string> Data;

        public Record(Dictionary<string, string> data)
        {
            this.Data = new Dictionary<string, string>(data);
        }
    }
}

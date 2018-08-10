using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlingBPlusTrees
{
    class Record
    {
        private List<string> Attributes { get; set; }
        private string Separator { get; set; } = ",";
        private string Terminator { get; set; } = ";";


        public Record(List<string> attributes)
        {
            this.Attributes = new List<string>(attributes);
        }

        public override string ToString()
        {
            return string.Join(this.Separator, this.Attributes) + this.Terminator;
        }
    }
}

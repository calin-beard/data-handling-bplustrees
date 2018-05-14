using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlingBPlusTrees
{
    //Decided to use one class for all types of nodes
    //in order to avoid the 'is' operator and cast operations
    class LeafNode : Node
    {
        public List<Record> Records { get; }
        public LeafNode NextNode { get; set; }

        public LeafNode(string k, int d, bool root = false) :
            base(k, d, root)
        {

        }
    }
}

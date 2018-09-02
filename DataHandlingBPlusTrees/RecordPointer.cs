using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlingBPlusTrees
{
    public class RecordPointer : IComparable
    {
        public int Block { get; set; }
        public int Offset { get; set; }

        public static RecordPointer Empty { get; set; } = new RecordPointer();
        static RecordPointer()
        {
            Empty.Block = -1;
            Empty.Offset = -1;
        }

        public RecordPointer() { }

        public RecordPointer(int _block, int _offset)
        {
            this.Block = _block;
            this.Offset = _offset;
        }

        public RecordPointer(Tuple<int, int> block_offset)
        {
            this.Block = block_offset.Item1;
            this.Offset = block_offset.Item2;
        }

        public int CompareTo(object obj)
        {
            RecordPointer rp = (RecordPointer)obj;
            return this.Block.CompareTo(rp.Block) != 0 ? this.Block.CompareTo(rp.Block) :
                this.Offset.CompareTo(rp.Offset);
        }
    }
}

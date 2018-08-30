using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlingBPlusTrees
{
    public abstract class SerializableRecord<R>
    {
        public BlockCache Cache { get; set; }

        public abstract int RecordSize();
        public abstract R ReadRecord(Block b, int offset);
        public abstract void WriteRecord(R record, Block b, int offset);
        public abstract string PathName();

        public SerializableRecord()
        {
            Cache = new BlockCache(PathName());
        }

        int MaxRecords()
        {
            return 4069 / RecordSize();
        }

        R GetRecord(int block, int offset)
        {
            return ReadRecord(Cache.GetBlock(block), offset);
        }

        void SetRecord(R record, int block, int offset)
        {
            WriteRecord(record, Cache.GetBlock(block), offset);
        }
    }
}

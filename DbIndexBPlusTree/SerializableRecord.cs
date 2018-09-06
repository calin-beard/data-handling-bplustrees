using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DbIndexBPlusTree
{
    public abstract class SerializableRecord<R>
    {
        public abstract BlockCache GetCache();
        public abstract int RecordSize();
        public abstract Block CreateEmptyBlock();
        protected abstract R ReadRecord(Block b, int offset);
        public abstract R GetEmptyRecord();
        protected abstract void WriteRecord(R record, Block b, int offset);
        public abstract string GetPathName();

        // Is named BlockSize but returns FileSize where the cache is flushed
        public int BlockSize()
        {
            Console.WriteLine("File size from block is " + this.GetCache().GetFileSize());
            return this.GetCache().GetFileSize();
        }

        public int MaxRecords()
        {
            return Block.Size() / this.RecordSize();
        }

        public R GetRecord(int block, int offset)
        {
            return ReadRecord(GetCache().GetBlock(block), offset);
        }

        public void SetRecord(R record, int block, int offset)
        {
            this.WriteRecord(record, GetCache().GetBlock(block), offset);
        }

        public void DeleteRecord(int block, int offset)
        {
            SetRecord(this.GetEmptyRecord(), block, offset);
        }
    }
}

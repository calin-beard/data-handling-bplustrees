using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlingBPlusTrees
{
    public class BlockCache
    {
        public int FileSize { get; set; } = 0;
        private const short SIZE = 4;
        private int[] idx;
        private Block[] blocks;
        private int oldest;
        private string pathName;

        public BlockCache(string _pathName)
        {
            this.pathName = _pathName;
            if (File.Exists(Employee.PathName()))
            {
                this.FileSize = (int)(new FileInfo(Employee.PathName())).Length;
            }
            else
            {
                using (FileStream f = File.Create(this.pathName))
                {
                    this.FileSize = (int)f.Length / Block.Size();
                    Console.WriteLine("Initial file size is " + this.FileSize);
                }
            }
            oldest = 0;
            idx = new int[SIZE];
            blocks = new Block[SIZE];
            for (int i = 0; i < SIZE; i++)
            {
                blocks[i] = new Block();
                idx[i] = -1;
            }
        }

        public Block GetBlock(int block)
        {
            for (int i = 0; i < SIZE; i++)
                if (idx[i] == block) return blocks[i];
            this.FlushLastBlock();
            Console.WriteLine("Reading block " + block);
            return this.ReadBlock(block);
        }

        private void FlushLastBlock()
        {
            if (idx[oldest] < 0) return;
            try
            {
                using (FileStream fs = new FileStream(pathName, FileMode.Open))
                {
                    fs.Seek(idx[oldest] * 4096, SeekOrigin.Begin);
                    Console.WriteLine("Flushing block " + idx[oldest]);
                    fs.Write(blocks[oldest].Bytes, 0, blocks[oldest].Bytes.Length);
                    fs.Flush();
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("---" + e.Message);
                Console.WriteLine("---" + e.StackTrace);
            }
        }

        public void FlushAllCachedBlocks()
        {
            for (int i = 0; i < SIZE; i++)
            {
                this.FlushLastBlock();
                idx[oldest] = -1;
                oldest = (oldest + 1) % SIZE;
            }
        }

        private Block ReadBlock(int block)
        {
            Block ob = blocks[oldest];
            try
            {
                using (FileStream fs = new FileStream(pathName, FileMode.OpenOrCreate))
                {
                    fs.Seek(block * Block.Size(), SeekOrigin.Begin);
                    fs.Read(ob.Bytes, 0, Block.Size());
                    idx[oldest] = block;
                    oldest = (oldest + 1) % SIZE;
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("---" + e.Message);
                Console.WriteLine("---" + e.StackTrace);
            }
            return ob;
        }
    }
}

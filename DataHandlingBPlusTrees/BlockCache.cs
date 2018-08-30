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
        private const short SIZE = 4;
        private int[] idx;
        private Block[] blocks;
        private int oldest;
        private string pathName;

        public BlockCache(string _pathName)
        {
            this.pathName = _pathName;
            oldest = 0;
            idx = new int[SIZE];
            for (int i = 0; i < SIZE; i++) idx[i] = -1;
            blocks = new Block[SIZE];
            for (int i = 0; i < SIZE; i++) blocks[i] = new Block();
        }

        public Block GetBlock(int number)
        {
            int where = Array.BinarySearch(idx, number);
            if (where >= 0) return blocks[where];
            this.FlushLastBlock();
            return this.ReadBlock(number);
        }

        private void FlushLastBlock()
        {
            if (idx[oldest] < 0) return;
            try
            {
                using (FileStream fs = new FileStream(pathName, FileMode.Open))
                {
                    fs.Write(blocks[oldest].Bytes, idx[oldest] * 4096, 4096);
                    fs.Flush();
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("---" + e.Message);
                Console.WriteLine("---" + e.StackTrace);
            }
        }

        private Block ReadBlock(int number)
        {
            Block ob = blocks[oldest];
            try
            {
                using (FileStream fs = new FileStream(pathName, FileMode.OpenOrCreate))
                {
                    fs.Read(ob.Bytes, number * 4096, 4096);
                    idx[oldest] = number;
                    number++;
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

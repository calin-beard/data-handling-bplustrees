using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlingBPlusTrees
{
    class FileManager
    {
        public string Path { get; set; }
        private int Block { get; set; } = 4096;
        //public string 

        public FileManager(string path)
        {
            this.Path = path;
        }

        /// <summary>
        /// Creates file that stores a relation
        /// </summary>
        /// <param name="recordLength">The fixed length of the record stored within the file</param>
        public void CreateRelation(int recordLength)
        {
            if (this.Block % recordLength != 0)
            {
                throw new Exception("The record lenght must be a divisor of the block size: " + this.Block);
            }


        }

        public void Write(string record)
        {
            //string path = Directory.GetCurrentDirectory() + @"\test";
            //Console.WriteLine("------------" + path);
            using (FileStream fs = new FileStream(this.Path, FileMode.OpenOrCreate))
            {
                Byte[] info = new UTF8Encoding(true).GetBytes(record);
                for (int i = info.Length; i < this.Block; i++)
                {
                    info[i] = new UTF8Encoding(true).GetBytes('\0');
                }
                Console.WriteLine("-------------" + info.Length);
                if (info.Length <= this.Block)
                {
                    fs.Write(info, 0, info.Length);
                }
                else
                {
                    throw new Exception("Record too large. Should be max " + this.Block + " bytes (chars)");
                }
            }
        }
    }
}

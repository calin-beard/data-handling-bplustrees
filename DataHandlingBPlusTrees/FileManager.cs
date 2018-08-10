using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataHandlingBPlusTrees
{
    class RelationFile
    {
        public string Path { get; set; }
        private int Block { get; set; } = 4096;

        public RelationFile(string path)
        {
            this.Path = path;
            this.Create(64);
        }

        /// <summary>
        /// Creates file that stores a relation
        /// </summary>
        /// <param name="PointerLength">The fixed length of the Pointer stored within the file</param>
        public void Create(int PointerLength)
        {
            if (this.Block % PointerLength != 0)
            {
                throw new Exception("The Pointer lenght must be a divisor of the block size: " + this.Block);
            }

            this.Write("H,5006");
        }

        /// <summary>
        /// Write an Pointer to the file created by this class
        /// </summary>
        /// <param name="Pointer">The information to be written</param>
        public void Write(string record)
        {
            using (FileStream fs = new FileStream(this.Path, FileMode.OpenOrCreate))
            {
                fs.Seek(0, SeekOrigin.End);
                Byte[] buffer = new Byte[4096];
                Byte[] info = new UTF8Encoding(true).GetBytes(record.ToString());
                Array.Copy(info, buffer, info.Length);
                for (int i = buffer.Length; i < this.Block; i++)
                {
                    buffer[i] = new UTF8Encoding(true).GetBytes("\0")[0];
                }
                Console.WriteLine("-------------" + buffer.Length);
                if (buffer.Length <= this.Block)
                {
                    fs.Write(buffer, 0, buffer.Length);
                }
                else
                {
                    throw new Exception("Pointer too large. Should be max " + this.Block + " bytes (chars)");
                }
            }
        }
    }
}
